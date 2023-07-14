using BackendGVK.Controllers;
using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Transactions;
using System.Security.Claims;
using System.Xml.Linq;

namespace BackendGVK.Services.CloudService
{
    public class CloudManager : ICloud
    {
        private const string HOME = "/home";
        private readonly IGraphClient _clientGraph;
        private readonly IDateProvider _dateTime;

        public GraphSet<FileModel> Files { get; set; }
        public GraphSet<DirectoryModel> Directories { get; set; }

        public CloudManager(IGraphClient clientGraph, IDateProvider dateProvider)
        {
            _dateTime = dateProvider;
            _clientGraph = clientGraph;
            Files = new GraphSet<FileModel>(ElementTypes.File, clientGraph);
            Directories = new GraphSet<DirectoryModel>(ElementTypes.Directory, clientGraph);
        }

        public async Task<bool> AddFileAsync(string userId, FileModel file, CloudInputModel input)
        {
            if (file == null || input==null || input.DestinationId == null || userId == null) throw new ArgumentNullException();
            file.isAdded = true;

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ Id : $elementId }})")
                .Merge($"(d)-[:HAS]->(nf:{ElementTypes.File} {{UntrustedName:$fileInstance.UntrustedName}})") 
                .OnMatch()
                .Set("nf.isAdded = false")
                .OnCreate()
                .Set("nf = $fileInstance")
                .WithParams(new
                {
                    id = userId,
                    elementId = input.DestinationId,
                    fileInstance = file
                })
                .Return(nf => nf.As<FileModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

        public async Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, CloudInputModel input)
        {
            if (dir == null || input == null || userId == null || input.DestinationId == null ) throw new ArgumentNullException();
            
            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ Id : $elementId }})")
                .Merge($"(d)-[:HAS]->(nd:{ElementTypes.Directory}  {{UntrustedName:$dirInstance.UntrustedName}} )")
                .OnMatch()
                .Set("nd.isAdded = false")
                .OnCreate()
                .Set("nd = $dirInstance")
                //.Set("nd.Type = $dirInstance.Type")
                .WithParams(new
                {
                    id = userId,
                    elementId = input.DestinationId,
                    dirInstance = dir
                })
                .Return(nd => nd.As<DirectoryModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!! remove method?
        public async Task<bool> ChangeNameAsync(string userId, string oldName, string currentName, ElementTypes type)
        {
            if (userId == null || oldName == null || currentName == null) throw new ArgumentNullException();
            if(oldName=="home") return false;

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{Id: $id }})-[*]->(e:{type} {{UntrustedName : $name }})")
                .Set("e.UntrustedName = $curName")
                .WithParams(new
                {
                    id = userId,
                    type = type.ToString(),
                    name = oldName,
                    curName = currentName
                })
                .Return(e => e.As<Element>().UntrustedName)
                .ResultsAsync;

            string result = results.FirstOrDefault()!;
            if(result == null || result!=currentName) return false;

            return true;
        }

        public async Task<InternalElements> GetElementsAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input == null || input.TargetId==null) throw new ArgumentNullException();

            string homeDirId = await GetHomeDirIdAsync(userId);

            if (homeDirId == null) return null!;

            if (input.TargetId == homeDirId)
            {
                var dirsTask = _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(dirs:{ElementTypes.Directory})")
                    .Where("dirs.DeleteDate is NULL")
                    .WithParam("id", userId)
                    .Return(dirs => dirs.CollectAs<DirectoryModel>())
                    .ResultsAsync;

                var filesTask = _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(files:{ElementTypes.File})")
                    .Where("files.DeleteDate is NULL")
                    .WithParam("id", userId)
                    .Return(files => files.CollectAs<FileModel>())
                    .ResultsAsync;

                var sharedTask = _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:ACCESS]->(shared:{ElementTypes.Directory})")
                    .Set("shared.isShared = true")
                    .WithParam("id", userId)
                    .Return(shared => shared.CollectAs<DirectoryModel>())
                    .ResultsAsync;

                var dirsResult = await dirsTask;
                var filesResult = await filesTask;
                var sharedResult = await sharedTask;


                var dirs = dirsResult.FirstOrDefault()!;
                var files = filesResult.FirstOrDefault()!;
                var shared = sharedResult.FirstOrDefault()!;

                var output = new InternalElements
                {
                    Files = files,
                    Directories = dirs,
                    Shared = shared
                };

                return output;
            }
            else
            {
                var result = await _clientGraph.Cypher
                    .Match($"(u:User {{ Id: $id }})-[*]->(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                    .OptionalMatch($"(d)-[:HAS]->(files:{ElementTypes.File})")
                    .OptionalMatch($"(d)-[:HAS]->(dirs:{ElementTypes.Directory})")
                    .Where("dirs.DeleteDate is NULL")
                    .AndWhere("files.DeleteDate is NULL")
                    .WithParams(new
                    {
                        id = userId,
                        dirId = input.TargetId
                    })
                    .Return((files, dirs) => new
                    {
                        Files = files.CollectAs<FileModel>(),
                        Directories = dirs.CollectAs<DirectoryModel>(),
                    })
                    .ResultsAsync;

                var elements = result.FirstOrDefault();
                if (elements == null) return null!;

                var output = new InternalElements
                {
                    Files = elements.Files,
                    Directories = elements.Directories
                };

                return output;
            }
        }
        public async Task<string> GetPathAsync(string userId, string elementId, ElementTypes type)
        {
            if (userId == null || elementId == null) throw new ArgumentNullException();

            var res = await _clientGraph.Cypher
                .Match($"(u:User {{ Id : $id }})-[*]->(e:{type} {{ Id : $elementId }})")
                .WithParams(new
                {
                    id = userId,
                    elementId
                })
                .Return(e => e.As<Element>().CloudPath)
                .ResultsAsync;

            return res.FirstOrDefault()!;
        }
        public async Task MoveToAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input==null || input.TargetId == null || input.DestinationId == null) throw new ArgumentNullException();

            var txcClient = (ITransactionalGraphClient)_clientGraph;
            using (var tx = txcClient.BeginTransaction())
            {
                await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS*]->(e:{input.Type} {{ Id :$targetId }})")
                    .Match("(e)<-[r:HAS]-()")
                    .OptionalMatch($"(u)-[:HAS*]->(d:{ElementTypes.Directory} {{ Id : $destId }})")
                    .WithParams(new
                    {
                        id = userId,
                        targetId = input.TargetId,
                        destId = input.DestinationId
                    })
                    .Delete("r")
                    .Create("(d)-[:HAS]->(e)")
                    .ExecuteWithoutResultsAsync();

                await tx.CommitAsync();
            }
        }
        public async Task RemoveAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input==null || input.TargetId == null) throw new ArgumentNullException();

            string homeDirId = await GetHomeDirIdAsync(userId);
            if (homeDirId == input.TargetId) throw new InvalidOperationException("Can not remove home directory.");

            var deleteTime = _dateTime.GetCustomDateTime(days: 30);

            await RemoveAccessAsync(input);

            await _clientGraph.Cypher
                .Match($"(:User {{Id:$id}})-[:HAS*]->(e:{input.Type} {{ Id : $targetId }})")
                .Set("e.DeleteDate=$date, e.isShared=false")
                .WithParams(new
                {
                    id = userId,
                    targetId = input.TargetId,
                    date = deleteTime.Ticks
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task DeleteAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input==null || input.TargetId==null) throw new ArgumentNullException();

            string homeDirId = await GetHomeDirIdAsync(userId);
            if (homeDirId == input.TargetId) throw new InvalidOperationException("Can not delete home directory.");

            var deleteTime = _dateTime.GetCurrentDate();

            await _clientGraph.Cypher
                .Match($"(:User {{ Id : $id }})-[*]->(e:{input.Type} {{ Id : $targetId }})")
                .Set("e.DeleteDate = $date")
                .WithParams(new
                {
                    id = userId,
                    targetId = input.TargetId,
                    date = deleteTime.Ticks
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task CopyToAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input==null || input.TargetId == null || input.DestinationId == null) throw new ArgumentNullException();
            if(input.DestinationPath == null) throw new ArgumentNullException();    

            if (input.Type==ElementTypes.File)
            {
                var files = await Files.Query.For(userId).Where(nameof(FileModel.Id), input.TargetId).ExecuteAsync();
                var file4Copy = files.FirstOrDefault();
                if (file4Copy == null) return;

                file4Copy.Id = Guid.NewGuid().ToString();
                file4Copy.isShared = false;
                file4Copy.OwnerId = userId;
                file4Copy.CloudPath = Path.Combine(input.DestinationPath, file4Copy.UntrustedName);

                await AddFileAsync(userId, file4Copy, input);
                return;
            }

            var dirs = await Directories.Query.For(userId).Where(nameof(DirectoryModel.Id), input.TargetId).ExecuteAsync();
            var dir4Copy = dirs.FirstOrDefault();

            if (dir4Copy != null)
            {
                dir4Copy.Id = Guid.NewGuid().ToString();
                dir4Copy.isShared = false;
                dir4Copy.CloudPath = Path.Combine(input.DestinationPath, dir4Copy.UntrustedName);
                dir4Copy.OwnerId = userId;

                bool isAdded=await AddDirectoryAsync(userId, dir4Copy, input);
                if (!isAdded) return;

                Stack<DirectoryModel> stack = new Stack<DirectoryModel>();
                stack.Push(dir4Copy);
                InternalElements internalElements;

                while (stack.Count > 0)
                {
                    dir4Copy = stack.Pop();
                    input.TargetPath = input.DestinationPath = dir4Copy.CloudPath;
                    input.TargetId = input.DestinationId = dir4Copy.Id;

                    internalElements = await GetElementsAsync(userId, input);

                    foreach (var dir in internalElements.Directories)
                    {
                        dir.Id = Guid.NewGuid().ToString();
                        dir.isShared = false;
                        dir.OwnerId = userId;
                        dir.CloudPath = Path.Combine(input.DestinationPath, dir.UntrustedName);

                        stack.Push(dir);
                        input.TargetPath = dir.CloudPath;
                        input.TargetId = dir.Id;
                        await AddDirectoryAsync(userId, dir, input);
                    }
                    foreach (var shared in internalElements.Shared)
                    {
                        shared.Id = Guid.NewGuid().ToString();
                        shared.isShared = false;
                        shared.OwnerId = userId;
                        shared.CloudPath = Path.Combine(input.DestinationPath, shared.UntrustedName);

                        stack.Push(shared);
                        input.TargetPath = shared.CloudPath;
                        input.TargetId = shared.Id;
                        await AddDirectoryAsync(userId, shared, input);
                    }
                    foreach (var file in internalElements.Files)
                    {
                        file.Id = Guid.NewGuid().ToString();
                        file.isShared = false;
                        file.OwnerId = userId;
                        file.CloudPath = Path.Combine(input.DestinationPath, file.UntrustedName);

                        input.TargetId = file.Id;
                        input.TargetPath = file.CloudPath;
                        await AddFileAsync(userId, file, input);
                    }
                }
            }
        }

        public async Task MoveToAccessModeAsync(string userId, CloudInputModel input)
        {
            if (userId == null || input==null || input.TargetId == null || input.DestinationId == null) throw new ArgumentNullException();

            var directories = await _clientGraph.Cypher
                    .OptionalMatch($"(:User {{ Id : $id }})-[:ACCESS]->()-[*]->(d:{ElementTypes.Directory} {{ Id : $destId }})")
                    .WithParams(new
                    {
                        id = userId,
                        destId = input.DestinationId
                    })
                    .Return(e => e.As<Element>().CloudPath)
                    .Union()
                    .OptionalMatch($"(:User {{ Id : $id }})-[:ACCESS]->(d:{ElementTypes.Directory} {{ Id : $destId }})")
                    .Return(d => d.As<Element>().CloudPath)
                    .ResultsAsync;

            string dirTargetId = directories.FirstOrDefault()!;
            if (dirTargetId != null)
            {
                await _clientGraph.Cypher
                    .Match("(u:User {Id : $id})")
                    .Match($"(u)-[:ACCESS]->()-[*]->(e:{input.Type} {{ Id : $elementId }})")
                    .Match("(e)<-[r:HAS]-()")
                    .Match($"(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                    .WithParams(new
                    {
                        elementId = input.TargetId,
                        dirId = dirTargetId
                    })
                    .Delete("r")
                    .Create("(d)-[:HAS]->(e)")
                    .ExecuteWithoutResultsAsync();
            }
        }

        public async Task SaveChangesAsync()
        {

        }
        public async Task RemoveAccessAsync(CloudInputModel input, string forUserId = null!)
        {
            if (input == null || input.TargetId==null) throw new ArgumentNullException();

            if(forUserId == null)
            {
                await _clientGraph.Cypher
                    .Match($"(e:Directory {{ Id : $id }})")
                    .OptionalMatch("()-[r:ACCESS]->(e)")
                    .WithParams(new
                    {
                        id = input.TargetId
                    })
                    .Delete("r")
                    .ExecuteWithoutResultsAsync();

            }
            else
            {
                await _clientGraph.Cypher
                    .Match($"(e:Directory {{ Id : $dirId }})")
                    .Match("(u:User { Id : $id })")
                    .OptionalMatch("(u)-[r:ACCESS]->(e)")
                    .WithParams(new
                    {
                        id = forUserId,
                        dirId = input.TargetId
                    })
                    .Delete("r")
                    .ExecuteWithoutResultsAsync();
            }
        }

        public async Task CreateHomeDirAsync(string userId, string email)
        {
            if (userId == null || email == null) throw new ArgumentNullException();

            if (await ExistsUserAsync(email))
            {
                await _clientGraph.Cypher
                    .Match("(u:User { Email = $email})")
                    .Set("(u.Id = $id")
                    .Merge($"(u)-[:HAS]->(d:{ElementTypes.Directory} {{ Id: $dirId, UntrustedName: \"home\", CloudPath : {HOME} }})")
                    .WithParams(new
                    {
                        dirId = Guid.NewGuid().ToString(),    
                        id = userId,
                        email,
                    })
                    .ExecuteWithoutResultsAsync();
            }
            else
            {
                await _clientGraph.Cypher
                    .Merge($"(u:User {{ Id: $id , Email : $email }})-[:HAS]->(d:{ElementTypes.Directory} {{ Id: $dirId, UntrustedName: \"home\", CloudPath : {HOME} }})")
                    .WithParams(new
                    {
                        dirId = Guid.NewGuid().ToString(),
                        id = userId,
                        email,
                    })
                    .ExecuteWithoutResultsAsync();
            }
        }
        
        public async Task RemoveHomeDirAsync(string userId)//!!!!!! переделать
        {
            if (userId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id:  $id }})-[:HAS]->(d:{ElementTypes.Directory})")
                .WithParams(new
                {
                    id = userId,
                })
                .DetachDelete("d,u")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<bool> isOwnerAsync(string userId, CloudInputModel inputModel)
        {
            return await isAllowedNode(true, userId, inputModel);
        }

        public async Task<bool> HasAccessAsync(string userId, CloudInputModel inputModel)
        {
            return await isAllowedNode(false, userId, inputModel);
        }

        private async Task<bool> isAllowedNode(bool ownerCheck, string userId, CloudInputModel input)
        {
            string r = "[*]";
            if (ownerCheck) r = "[HAS*]";

            var values = await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : $id })")
                .OptionalMatch($"(e:{input.Type} {{ Id : $elementId }})")
                .Match($"p = shortestPath((u)-{r}->(e))")
                .WithParams(new { id = userId, elementId = input.TargetId })
                .Return(p => p.Length())
                .ResultsAsync;
            long? value = values.FirstOrDefault();

            return value>0;
        }

        public async Task GrantAccessForAsync(ClaimsPrincipal principal, string toEmail, DirectoryModel directory)
        {
            if (principal == null || toEmail == null || directory == null) throw new ArgumentNullException();

            string fromName = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value!;
            string fromEmail = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value!;
            if (fromName == null || fromEmail == null) throw new NullReferenceException("Claims are invalid");

            InvitationModel invitation = new InvitationModel
            {
                Id = Guid.NewGuid().ToString(),
                DirectoryId = directory.Id,
                Directory = directory.UntrustedName,
                From = fromName,
            };

            await _clientGraph.Cypher
                .Match("(from:User { Email : $from })")
                .Merge("(to:User { Email : $to })")
                .Merge("(from)-[:INVITED {$invitation} ]->(to)")
                .WithParams(new
                {
                    from = fromEmail,
                    to = toEmail,
                    invitation
                })
                .ExecuteWithoutResultsAsync();
        }

        public async Task<bool> ExistsUserAsync(string email)
        {
            if(email == null) throw new ArgumentNullException();

            var values = await _clientGraph.Cypher
                .OptionalMatch("(u:User { Email : $email })")
                .WithParam("email", email)
                .Return(u => u.As<string>())
                .ResultsAsync;

            bool value = values.FirstOrDefault() == null ? false : true;

            return value;
        }

        public async Task<IEnumerable<InvitationModel>> GetInvitationsAsync(string userId)
        {
            if(userId == null) throw new ArgumentNullException();

            var invitations = await _clientGraph.Cypher
                .Match("(u:User { Id : $id })")
                .OptionalMatch("(u)<-[r:INVITED]-()")
                .WithParam("id", userId)
                .Return(r => r.As<InvitationModel>())
                .ResultsAsync;

            return invitations;
        }

        public async Task AcceptInvitationAsync(string toUserId, InvitationModel invitation)
        {
            if (toUserId == null || invitation == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match("()-[r:INVITED { Id : $invId}]->()")
                .Match("(u:User { Id : $userId })")
                .Match($"(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                .Merge("(u)-[:ACCESS]->(d)")
                .WithParams(new
                {
                    userId = toUserId,
                    invId = invitation.Id,
                    dirId = invitation.DirectoryId
                })
                .Delete("r")
                .ExecuteWithoutResultsAsync();
        }

        public async Task<string> DeleteInvitationAsync(InvitationModel invitation)
        {
            if(invitation== null) throw new ArgumentNullException();

            var result = await _clientGraph.Cypher
                .OptionalMatch("()<-[r:INVITED {Id : $inviteId}]-()")
                .WithParams(new
                {
                    inviteId = invitation.Id
                })
                .Delete("r")
                .Return(r => r.As<InvitationModel>().Id)
                .ResultsAsync;

            return result.FirstOrDefault()!;
        }
        public async Task<string> GetHomeDirIdAsync(string userId)
        {
            var result = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id : $userId }})-[:HAS]->(d:{ElementTypes.Directory})")
                .WithParam("userId", userId)
                .Return(d => d.As<DirectoryModel>().Id)
                .ResultsAsync;

            return result.FirstOrDefault()!;
        }

        public async Task<string> GetDirSizeAsync(string userId, string dirId)
        {
            var res = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id : $id }}-[*]->(d:{ElementTypes.Directory} {{Id :$dirId}})")
                .OptionalMatch($"(d)-[:HAS*]->(f:{ElementTypes.File})")
                .WithParams(new
                {
                    id = userId,
                    dirId = dirId
                })
                .Return(f => f.As<FileModel>().Size)
                .ResultsAsync;

            ulong result = 0;
            foreach (var fileSize in res)
            {
                result += ulong.Parse(fileSize);
            }

            return result.ToString();
        }

        public async Task<InternalElements> GetRemovedElements(string userId)
        {
            if (userId == null) throw new ArgumentNullException();

            var date = _dateTime.GetCurrentDate();

            var dirsTask = _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(dirs:{ElementTypes.Directory})")
                .Where("dirs.DeleteDate < $current")
                .WithParams(new
                {
                    id = userId,
                    current = date.Ticks
                })
                .Return(dirs => dirs.CollectAs<DirectoryModel>())
                .ResultsAsync;

            var filesTask = _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(files:{ElementTypes.File})")
                .Where("files.DeleteDate < $current")
                .WithParams(new
                {
                    id = userId,
                    current = date.Ticks
                })
                .Return(files => files.CollectAs<FileModel>())
                .ResultsAsync;

            var dirs = await dirsTask;
            var files = await filesTask;

            var internalElements = new InternalElements
            {
                Files = files.FirstOrDefault()!,
                Directories = dirs.FirstOrDefault()!,
            };

            return internalElements;
        }

        public async Task RestoreElementAsync(string userId, CloudInputModel input)
        {
            if(userId == null || input == null || input.TargetId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match($"(:User {{Id : $id}})-[:HAS*]->(e:{input.Type} {{ Id : $elementId}})")
                .Set("e.DeleteDate=NULL")
                .WithParams(new
                {
                    id = userId,
                    elementId = input.TargetId,
                })
                .ExecuteWithoutResultsAsync();
        }
    }
}
