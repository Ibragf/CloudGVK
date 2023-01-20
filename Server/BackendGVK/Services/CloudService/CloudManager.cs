using BackendGVK.Controllers;
using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Transactions;

namespace BackendGVK.Services.CloudService
{
    public class CloudManager : ICloud
    {
        private readonly IGraphClient _clientGraph;
        public CloudManager(IGraphClient clientGraph)
        {
            _clientGraph = clientGraph;
        }
        public async Task<bool> AddFileAsync(string userId, FileModel file, string destination)
        {
            if (file == null || destination == null || userId == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ UntrustedName : $dirname }}")
                .Merge($"(d)-[:HAS]->(f:{ElementTypes.File} $fileInstance )")
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
                .Return(e => e.As<FileModel>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

        public async Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string destination)
        {
            if (dir == null || destination == null || userId == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch($"(:User {{ Id: $id }})-[r*]->(d:{ElementTypes.Directory} {{ UntrustedName : $dirname }}")
                .Merge($"(d)-[:HAS]->(nd:{ElementTypes.Directory} $dirInstance )")
                .OnCreate()
                .Set("nd.isAdded = true")
                .OnMatch()
                .Set("nd.isAdded = false")
                .WithParams(new
                {
                    id = userId,
                    dirname = destination,
                    dirInstance = dir
                })
                .Return(e => e.As<DirectoryModel>().isAdded)
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

            var res = await _clientGraph.Cypher
                .Match($"(u:User {{ Id : $id }})-[*]->(e:{type} {{ UntrustedName : $name }})")
                .WithParams(new
                {
                    id = userId,
                    type = type.ToString(),
                    name
                })
                .Return(e => e.As<Element>().Path)
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

        public async Task AddAccessAsync(string elementId, string userId)
        {
            if (elementId == null || userId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match($"(e:Directory {{ Id : $elementId }})")
                .Match("(u:User { Id : $id }")
                .Merge("(u)-[:ACCESS]->(e)")
                .WithParams(new
                {
                    id = userId,
                    elementId,
                })
                .ExecuteWithoutResultsAsync();
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

        public async Task CreateHomeDirAsync(string userId)
        {
            if (userId == null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Merge($"(u:User {{ Id: $id }})-[:HAS]->(d:{ElementTypes.Directory} {{ UntrustedName: \"home\" }})")
                .WithParams(new
                {
                    id = userId,
                })
                .ExecuteWithoutResultsAsync();
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
    }
}
