using BackendGVK.Controllers;
using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Transactions;
using System.Security.Claims;

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
        public async Task<bool> AddFileAsync(string userId, FileModel file, string destination)
        {
            if (file == null || destination == null || userId == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ UntrustedName : $dirname }}")
                .Merge($"(d)-[:HAS]->(f:{ElementTypes.File} {{ $fileInstance }})")
                .OnCreate()
                .Set("f.isAdded = true")
                .OnMatch()
                .Set("f.isAdded = false")
                .WithParams(new
                {
                    id = userId,
                    dirname = destination,
                    fileInstance = file
                })
                .Return(f => f.As<FileModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

        public async Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string destination)
        {
            if (dir == null || destination == null || userId == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ UntrustedName : $dirname }})")
                .Merge($"(d)-[:HAS]->(nd:{ElementTypes.Directory}  {{UntrustedName:$dirInstance.UntrustedName}} )")
                .OnMatch()
                .Set("nd.isAdded = false")
                .OnCreate()
                .Set("nd = $dirInstance")
                //.Set("nd.Type = $dirInstance.Type")
                .WithParams(new
                {
                    id = userId,
                    dirname = destination,
                    dirInstance = dir
                })
                .Return(nd => nd.As<DirectoryModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

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

        public async Task<OutputElements> GetElementsAsync(string userId, string directory)
        {
            if (userId == null || directory == null) throw new ArgumentNullException();

            if (directory == "home")
            {
                var res = await _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS]->()-[:HAS]->(files:{ElementTypes.File})")
                .OptionalMatch($"(u)-[:HAS]->()-[:HAS]->(dirs:{ElementTypes.Directory})")
                .OptionalMatch($"(u)-[:ACCESS]->(shared:{ElementTypes.Directory})")
                .Set("shared.isShared = true")
                .WithParam("id",userId)
                .Return((files, dirs, shared) => new {
                    Files = files.CollectAs<FileModel>(),
                    Directories = dirs.CollectAs<DirectoryModel>(),
                    Shared = shared.CollectAs<DirectoryModel>()
                 })
                .ResultsAsync;

                var elements = res.FirstOrDefault();
                if (elements == null) return null!;

                var output = new OutputElements
                {
                    Files = elements.Files,
                    Directories = elements.Directories,
                    Shared = elements.Shared
                };

                return output;
            }
            else
            {
                var res = await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id: $id }})-[*]->(d:{ElementTypes.Directory} {{ UntrustedName : $dirName }})")
                    .OptionalMatch($"(d)-[:HAS]->(files:{ElementTypes.File})")
                    .OptionalMatch($"(d)-[:HAS]->(dirs:{ElementTypes.Directory})")
                    .WithParams(new
                    {
                        id = userId,
                        dirName = directory
                    })
                    .Return((files, dirs) => new
                    {
                        Files = files.CollectAs<FileModel>(),
                        Directories = dirs.CollectAs<DirectoryModel>(),
                    })
                    .ResultsAsync;

                var elements = res.FirstOrDefault();
                if (elements == null) return null!;

                var output = new OutputElements
                {
                    Files = elements.Files,
                    Directories = elements.Directories
                };

                return output;
            }
        }

        public async Task<string> GetPathAsync(string userId, string name, ElementTypes type)
        {
            if (userId == null || name == null) throw new ArgumentNullException();
            if(name == "home")
            {
                return "/home";
            }

            var res = await _clientGraph.Cypher
                .Match($"(u:User {{ Id : $id }})-[*]->(e:{type} {{ UntrustedName : $name }})")
                .WithParams(new
                {
                    id = userId,
                    type = type.ToString(),
                    name
                })
                .Return(e => e.As<Element>().CloudPath)
                .ResultsAsync;

            return res.FirstOrDefault()!;
        }

        public async Task MoveToAsync(string userId, string name, string directoryDestination, ElementTypes type)
        {
            if (userId == null || name == null || directoryDestination == null) throw new ArgumentNullException();

            var txcClient = (ITransactionalGraphClient)_clientGraph;
            using (var tx = txcClient.BeginTransaction())
            {
                await _clientGraph.Cypher
                    .OptionalMatch($"(u:User {{ Id : $id }})-[:HAS*]->(e:{type} {{ UntrustedName :$ename }})")
                    .Match("(e)<-[r:HAS]-()")
                    .OptionalMatch($"(u)-[:HAS*]->(d:{ElementTypes.Directory} {{ UntrustedName : $destination }})")
                    .WithParams(new
                    {
                        id = userId,
                        ename = name,
                        destination = directoryDestination,
                    })
                    .Delete("r")
                    .Create("(d)-[:HAS]->(e)")
                    .ExecuteWithoutResultsAsync();

                await tx.CommitAsync();
            }
        }

        public async Task RemoveAsync(string userId, string name, ElementTypes type)
        {
            if (userId == null || name==null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match($"(:User {{ Id : $id }})-[*]->(e:{type} {{ UntrustedName : $ename }})")
                .OptionalMatch("(e)-[*]->(x)")
                .WithParams(new
                {
                    id = userId,
                    ename = name,
                })
                .DetachDelete("x,e")
                .ExecuteWithoutResultsAsync();
        }

        public async Task CopyToAsync(string userId, string name, string destination, ElementTypes type)
        {
            if (userId == null || name == null || destination == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id : $id }})-[*]->(e:{type} {{ UntrustedName : $ename }})")
                .OptionalMatch($"(u)-[*]->(d:{ElementTypes.Directory} {{ UntrustedName : $destination }})")
                .Merge("(d)-[:HAS]->(e)")
                .OnMatch("e.isAdded = false")
                .WithParams(new
                {
                    id = userId,
                    ename = name,
                    destination
                })
                .ExecuteWithoutResultsAsync();
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
                .Merge($"(u)-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName: \"home\" }})")
                .WithParams(new
                {
                    id = userId,
                    email
                })
                .ExecuteWithoutResultsAsync();
            }
            else
            {
                await _clientGraph.Cypher
                .Merge($"(u:User {{ Id: $id , Email : $email }})-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName: \"home\" }})")
                .WithParams(new
                {
                    id = userId,
                    email
                })
                .ExecuteWithoutResultsAsync();
            }
        }

        public async Task RemoveHomeDirAsync(string userId)
        {
            if (userId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .OptionalMatch($"(u:User {{ Id:  $id }})-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName : \"home\" }}")
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
            if (ownerCheck) r = "[:HAS*]";

            var values = await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : $id })")
                .OptionalMatch($"(e:{type} {{ Id : $elementId }}")
                .Call($"exists((u)-[{r}]->(e) as result")
                .WithParams(new { id = userId, elementId })
                .Return(result => result.As<bool>())
                .ResultsAsync;

            bool value = values.FirstOrDefault();

            return value;
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

            string invId = await DeleteInvitationAsync(invitation);
            if(invId == null) return;

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


    }
}
