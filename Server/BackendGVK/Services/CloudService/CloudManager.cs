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
        private readonly IGraphClient _clientGraph;

        public GraphSet<FileModel> Files { get; set; }
        public GraphSet<DirectoryModel> Directories { get; set; }

        public CloudManager(IGraphClient clientGraph)
        {
            _clientGraph = clientGraph;
            Files = new GraphSet<FileModel>(ElementTypes.File, clientGraph);
            Directories = new GraphSet<DirectoryModel>(ElementTypes.Directory, clientGraph);
        }
        public async Task<bool> AddFileAsync(string userId, FileModel file, string destinationId)
        {
            if (file == null || destinationId == null || userId == null) throw new ArgumentNullException();
            file.isAdded = true;

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                .Merge($"(d)-[:HAS]->(nf:{ElementTypes.File} {{UntrustedName:$fileInstance.UntrustedName}})") //!!!добавление файлов с одинаковыми именами??
                .OnMatch()
                .Set("nf.isAdded = false")
                .OnCreate()
                .Set("nf = $fileInstance")
                .WithParams(new
                {
                    id = userId,
                    dirId = destinationId,
                    fileInstance = file
                })
                .Return(nf => nf.As<FileModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

        public async Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string destinationId)
        {
            if (dir == null || destinationId == null || userId == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                .Merge($"(d)-[:HAS]->(nd:{ElementTypes.Directory}  {{UntrustedName:$dirInstance.UntrustedName}} )")
                .OnMatch()
                .Set("nd.isAdded = false")
                .OnCreate()
                .Set("nd = $dirInstance")
                //.Set("nd.Type = $dirInstance.Type")
                .WithParams(new
                {
                    id = userId,
                    dirId = destinationId,
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

        public async Task<InternalElements> GetElementsAsync(string userId, string directoryId)
        {
            if (userId == null || directoryId == null) throw new ArgumentNullException();

            var res = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{Id : $userId}})-[:HAS]->(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                .WithParams( new {
                    userId,
                    dirId = directoryId
                 })
                .Return(d => d.As<DirectoryModel>().UntrustedName)
                .ResultsAsync;
            bool isHomeDir = res.FirstOrDefault()==null ? false : true;
            if (isHomeDir)
            {
                var dirsResult = await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(dirs:{ElementTypes.Directory})")
                    .WithParam("id", userId)
                    .Return(dirs => dirs.CollectAs<DirectoryModel>())
                    .ResultsAsync;

                var filesResult = await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(files:{ElementTypes.File})")
                    .WithParam("id", userId)
                    .Return(files => files.CollectAs<Element>())
                    .ResultsAsync;

                var sharedResult = await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:ACCESS]->(shared:{ElementTypes.Directory})")
                    .Set("shared.isShared = true")
                    .WithParam("id", userId)
                    .Return(shared => shared.CollectAs<DirectoryModel>())
                    .ResultsAsync;

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
                    .WithParams(new
                    {
                        id = userId,
                        dirId = directoryId
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

        public async Task MoveToAsync(string userId, string elementId, string destinationId, ElementTypes type)
        {
            if (userId == null || elementId == null || destinationId == null) throw new ArgumentNullException();

            var txcClient = (ITransactionalGraphClient)_clientGraph;
            using (var tx = txcClient.BeginTransaction())
            {
                await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS*]->(e:{type} {{ Id :$elementId }})")
                    .Match("(e)<-[r:HAS]-()")
                    .OptionalMatch($"(u)-[:HAS*]->(d:{ElementTypes.Directory} {{ Id : $destinationId }})")
                    .WithParams(new
                    {
                        id = userId,
                        elementId,
                        destinationId
                    })
                    .Delete("r")
                    .Create("(d)-[:HAS]->(e)")
                    .ExecuteWithoutResultsAsync();

                await tx.CommitAsync();
            }
        }

        public async Task RemoveAsync(string userId, string elementId, ElementTypes type)
        {
            if (userId == null || elementId==null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match($"(:User {{ Id : $id }})-[*]->(e:{type} {{ Id : $elementId }})")
                .OptionalMatch("(e)-[*]->(x)")
                .WithParams(new
                {
                    id = userId,
                    elementId
                })
                .DetachDelete("x,e")
                .ExecuteWithoutResultsAsync();
        }

        public async Task CopyToAsync(string userId, string elementId, string destinationId, ElementTypes type)
        {
            if (userId == null || elementId == null || destinationId == null) throw new ArgumentNullException();

            var models = await Directories.Query.Where(nameof(DirectoryModel.Id), destinationId).ExecuteAsync();
            DirectoryModel destination = models.FirstOrDefault()!;

            if (type==ElementTypes.File && destination!=null)
            {
                await CopyFileAsync(userId, elementId, destination);
                return;
            }

            models = await Directories.Query.Where(nameof(DirectoryModel.Id), elementId).ExecuteAsync();
            var dir4Copy = models.FirstOrDefault();

            if (destination != null && dir4Copy != null)
            {
                dir4Copy.Id = Guid.NewGuid().ToString();
                dir4Copy.isShared = false;
                dir4Copy.CloudPath = Path.Combine(destination.CloudPath, dir4Copy.UntrustedName);
                destination.Size = (ulong.Parse(destination.Size) + ulong.Parse(dir4Copy.Size)).ToString();
                await AddDirectoryAsync(userId, dir4Copy, destination.Id);

                Stack<DirectoryModel> stack = new Stack<DirectoryModel>();
                stack.Push(dir4Copy);
                InternalElements internalElements;

                while (stack.Count > 0)
                {
                    dir4Copy = stack.Pop();
                    internalElements = await GetElementsAsync(userId, dir4Copy.Id);

                    foreach (var dir in internalElements.Directories)
                    {
                        dir.Id = Guid.NewGuid().ToString();
                        dir.isShared = false;
                        dir.CloudPath = Path.Combine(destination.CloudPath, dir.UntrustedName);

                        stack.Push(dir);
                        await AddDirectoryAsync(userId, dir, dir4Copy.Id);
                    }
                    foreach (var shared in internalElements.Shared)
                    {
                        shared.Id = Guid.NewGuid().ToString();
                        shared.isShared = false;
                        shared.CloudPath = Path.Combine(destination.CloudPath, shared.UntrustedName);

                        stack.Push(shared);
                        await AddDirectoryAsync(userId, shared, dir4Copy.Id);
                    }
                    foreach (var file in internalElements.Files)
                    {
                        file.Id = Guid.NewGuid().ToString();
                        file.CloudPath = Path.Combine(destination.CloudPath, file.UntrustedName);

                        await CopyFileAsync(userId, file.Id, dir4Copy);
                    }
                }
                await Directories.Query
                            .Where(nameof(DirectoryModel.Id), destination.Id)
                            .Update(nameof(DirectoryModel.Size), destination.Size)
                            .ExecuteAsync();
            }
        }

        private async Task CopyFileAsync(string userId, string elementId, DirectoryModel destination)
        {
            var result = await Files.Query.Where(nameof(FileModel.Id), elementId).ExecuteAsync();
            FileModel file = result.FirstOrDefault()!;

            if(file != null)
            {
                if (destination != null)
                {
                    file.Id = Guid.NewGuid().ToString();
                    file.isShared = false;
                    file.CloudPath = Path.Combine(destination.CloudPath, file.UntrustedName);
                    destination.Size = (ulong.Parse(destination.Size)+ulong.Parse(file.Size)).ToString();

                    await AddFileAsync(userId, file, destination.Id);
                    destination.Size = (ulong.Parse(destination.Size) + ulong.Parse(file.Size)).ToString();
                }
            }
        }

        public async Task MoveToAccessModeAsync(string userId, string elementId, string destinationId , ElementTypes type)
        {
            if (userId == null || elementId == null || destinationId == null) throw new ArgumentNullException();

            var element=await _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id : $id }})-[:ACCESS]->()-[*]->(e:{type} {{ Id : $elementId }})")
                .WithParams(new
                {
                    id = userId,
                    elementId
                })
                .Return(e => e.As<Element>().Id)
                .ResultsAsync;

            string targetId = element.FirstOrDefault()!;

            if (targetId != null)
            {
                var directory = await _clientGraph.Cypher
                    .OptionalMatch($"(:User {{ Id : $id }})-[:ACCESS]->()-[*]->(d:{ElementTypes.Directory} {{ Id : $destId }})")
                    .WithParams(new
                    {
                        id = userId,
                        destId = destinationId
                    })
                    .Return(e => e.As<Element>().Id)
                    .Union()
                    .OptionalMatch($"(:User {{ Id : $id }})-[:ACCESS]->(d:{ElementTypes.Directory} {{ Id : $destId2 }})")
                    .WithParams(new
                    {
                        id = userId,
                        destId2 = destinationId
                    })
                    .Return(d => d.As<Element>().Id)
                    .ResultsAsync;

                string dirTargetId = directory.FirstOrDefault()!;
                if(dirTargetId != null)
                {
                    await _clientGraph.Cypher
                        .Match($"(e:{type} {{ Id : $elementId }})")
                        .Match("(e)<-[r:HAS]-()")
                        .Match($"(d:{ElementTypes.Directory} {{ Id : $dirId }})")
                        .WithParams(new
                        {
                            elementId = targetId,
                            dirId = dirTargetId
                        })
                        .Delete("r")
                        .Create("(d)-[:HAS]->(e)")
                        .ExecuteWithoutResultsAsync();
                }
            }
        }

        public async Task SaveChangesAsync()
        {

        }
        public async Task RemoveAccessAsync(string elementId, string userId)
        {
            if (elementId == null || userId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match($"(e:Directory {{ Id : $elementId }})")
                .Match("(u:User { Id : $id }")
                .OptionalMatch("(u)-[r:ACCESS]->(e)")
                .WithParams(new
                {
                    id = userId,
                    elementId,
                })
                .Delete("r")
                .ExecuteWithoutResultsAsync();
        }

        public async Task CreateHomeDirAsync(string userId, string email)
        {
            if (userId == null || email == null) throw new ArgumentNullException();

            if (await ExistsUserAsync(email))
            {
                await _clientGraph.Cypher
                .Match("(u:User { Email = $email})")
                .Set("(u.Id = $id")
                .Merge($"(u)-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName: \"home\", Id : $dirId, CloudPath : \"/home\" }})")
                .WithParams(new
                {
                    id = userId,
                    email,
                    dirId = Guid.NewGuid().ToString()
                })
                .ExecuteWithoutResultsAsync();
            }
            else
            {
                await _clientGraph.Cypher
                .Merge($"(u:User {{ Id: $id , Email : $email }})-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName: \"home\", Id : $dirId, CloudPath : \"/home\" }})")
                .WithParams(new
                {
                    id = userId,
                    email,
                    dirId = Guid.NewGuid().ToString()
                })
                .ExecuteWithoutResultsAsync();
            }
        }

        public async Task RemoveHomeDirAsync(string userId)
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

        public async Task<bool> isOwnerAsync(string userId, string elementId, ElementTypes type)
        {
            return await isAllowedNode(true, userId, elementId, type);
        }

        public async Task<bool> HasAccessAsync(string userId, string elementId, ElementTypes type)
        {
            return await isAllowedNode(false, userId, elementId, type);
        }

        private async Task<bool> isAllowedNode(bool ownerCheck, string userId, string elementId,  ElementTypes type)
        {
            string r = "[*]";
            if (ownerCheck) r = "[HAS*]";

            var values = await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : $id })")
                .OptionalMatch($"(e:{type} {{ Id : $elementId }})")
                .Match($"p = shortestPath((u)-{r}->(e))")
                .WithParams(new { id = userId, elementId })
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
                Size = directory.Size
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
        public async Task<string> GetHomeDirId(string userId)
        {
            var result = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id : $userId }})-[:HAS]->(d:{ElementTypes.Directory})")
                .WithParam("userId", userId)
                .Return(d => d.As<DirectoryModel>().Id)
                .ResultsAsync;

            return result.FirstOrDefault()!;
        }
    }
}
