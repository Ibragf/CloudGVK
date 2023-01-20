using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.EntityFrameworkCore;
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
        public async Task<bool> AddAsync(ApplicationUser user, Element element, string directory)
        {
            if (element == null || directory == null) throw new ArgumentNullException();

            var results = await _clientGraph.Cypher
                .OptionalMatch("(:User { Id: {id} })-[r*]->(d:{directory} { UntrustedName : {dirname} }")
                .Merge("(d)-[:HAS]->(e:{type} {el} )")
                .OnCreate()
                .Set("isAdded = true")
                .OnMatch()
                .Set("isAdded = false")
                .WithParams(new
                {
                    id = user.Id,
                    type = element.Type.ToString(),
                    directory = ElementTypes.Directory.ToString(),
                    dirname = directory,
                    el = element
                })
                .Return(e => e.As<Element>().isAdded)
                .ResultsAsync;

            bool result = results.FirstOrDefault();

            return result;
        }

        public async Task<bool> ChangeNameAsync(ApplicationUser user, string oldName, string currentName, ElementTypes type)
        {
            if (user == null || oldName == null || currentName == null) throw new ArgumentNullException();
            if(oldName=="home") return false;

            var results = await _clientGraph.Cypher
                .OptionalMatch("(:User {Id: {id} })-[*]->(e:{type} {UntrustedName : {name} })")
                .Set("e.UntrustedName = {curName}")
                .WithParams(new
                {
                    id = user.Id,
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

        public async Task<IEnumerable<Element>> GetElementsAsync(ApplicationUser user, string directory)
        {
            if (user == null || directory == null) throw new ArgumentNullException();
            List<Element> result;

            if (directory == "home")
            {
                var res = await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : {id} })->[:HAS]->()-[:HAS]->(elements)")
                .WithParam("id",user.Id)
                .Return((elements) => elements.CollectAs<Element>())
                .Union()
                .OptionalMatch("(u:User { Id : {id} })-[:ACCESS]->(shared)")
                .Set("shared.isShared = true")
                .WithParam("id", user.Id)
                .Return(shared => shared.CollectAs<Element>())
                .ResultsAsync;

                result = new();
                var elements = res.FirstOrDefault();
                if (elements == null) return null!;

                result.AddRange(elements);
            }
            else
            {
                var res = await _clientGraph.Cypher
                    .OptionalMatch("(u:User { Id: {id} })-[*]->(d:{dirType} { UntrustedName : {dirName} })")
                    .OptionalMatch("(d)-[:HAS]->(elements)")
                    .WithParams(new
                    {
                        id = user.Id,
                        dirType = ElementTypes.Directory.ToString(),
                        dirName = directory
                    })
                    .Return(elements => elements.CollectAs<Element>()).ResultsAsync;

                result = new();
                var elements = res.FirstOrDefault();
                if (elements == null) return null!;

                result.AddRange(elements);
            }

            return result;
        }

        public async Task<string> GetPathAsync(ApplicationUser user, string name, ElementTypes type)
        {
            if (user == null || name == null) throw new ArgumentNullException();

            var res = await _clientGraph.Cypher
                .Match("(u:User { Id : {id} })-[*]->(e:{type} { UntrustedName : {ename} })")
                .WithParams(new
                {
                    id = user.Id,
                    type = type.ToString(),
                    ename = name
                })
                .Return(e => e.As<Element>().Path)
                .ResultsAsync;

            return res.FirstOrDefault()!;
        }

        public async Task MoveToAsync(ApplicationUser user, string name, string directoryDestination, ElementTypes type)
        {
            if (user == null || name == null || directoryDestination == null) throw new ArgumentNullException();

            var txcClient = (ITransactionalGraphClient)_clientGraph;
            using (var tx = txcClient.BeginTransaction())
            {
                await _clientGraph.Cypher
                    .OptionalMatch("(u:User { Id : {id} })-[:HAS*]->(e:{type} { UntrustedName :{ename} })")
                    .Match("(e)<-[r:HAS]-()")
                    .OptionalMatch("(u)-[:HAS*]->(d:{dirtype} { UntrustedName :{destination} })")
                    .WithParams(new
                    {
                        id = user.Id,
                        type = type.ToString(),
                        ename = name,
                        destination = directoryDestination,
                        dirtype = ElementTypes.Directory.ToString()
                    })
                    .Delete("r")
                    .Create("(d)-[:HAS]->(e)")
                    .ExecuteWithoutResultsAsync();

                await tx.CommitAsync();
            }
        }
        public async Task RemoveAsync(ApplicationUser user, string name, ElementTypes type)
        {
            if (user == null || name==null) throw new ArgumentNullException();

            await _clientGraph.Cypher
                .Match("(:User { Id : {id} })-[*]->(e:{type} { UntrustedName : {ename} })")
                .OptionalMatch("(e)-[*]->(x)")
                .WithParams(new
                {
                    id = user.Id,
                    type = type.ToString(),
                    ename = name,
                })
                .DetachDelete("x,e")
                .ExecuteWithoutResultsAsync();
        }
        public async Task CopyToAsync(ApplicationUser user, string name, string destination, ElementTypes type)
        {
            await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : {id} })-[*]->(e:{type} { UntrustedName : {ename} })")
                .OptionalMatch("(u)-[*]->(d:{dirType} { UntrustedName : {destination} })")
                .Merge("(d)-[:HAS]->(e)")
                .WithParams(new
                {
                    id = user.Id,
                    type = type.ToString(),
                    ename = name,
                    dirType = ElementTypes.Directory.ToString(),
                    destination
                })
                .ExecuteWithoutResultsAsync();
        }
        public async Task MoveToAccessModeAsync(ApplicationUser user, string name, string destination, ElementTypes type)
        {
            var element=await _clientGraph.Cypher
                .OptionalMatch("(u:User { Id : {id} })-[:ACCESS]->()-[*]->(e:{type} { Id : {elementId} })")
                .WithParams(new
                {
                    id = user.Id,
                    type = type.ToString(),
                    elementId = name
                })
                .Return(e => e.As<Element>().Id)
                .ResultsAsync;

            string targetId = element.FirstOrDefault()!;

            if (targetId != null)
            {
                var directory = await _clientGraph.Cypher
                    .OptionalMatch("(:User { Id : {id} })-[:ACCESS]->()-[*]->(e:{type} { Id : {elementId} })")
                    .WithParams(new
                    {
                        id = user.Id,
                        type = ElementTypes.Directory.ToString(),
                        elementId = destination
                    })
                    .Return(e => e.As<Element>().Id)
                    .Union()
                    .OptionalMatch("(:User { Id : {id} })-[:ACCESS]->(d:{dirType} { Id : {elementId} })")
                    .WithParams(new
                    {
                        id = user.Id,
                        dirType = ElementTypes.Directory.ToString(),
                        elementId = destination
                    })
                    .Return(d => d.As<Element>().Id)
                    .ResultsAsync;

                string dirTargetId = directory.FirstOrDefault()!;
                if(dirTargetId != null)
                {
                    await _clientGraph.Cypher
                        .Match("(e:{type} { Id : {elementId} })")
                        .Match("(e)<-[r:HAS]-()")
                        .Match("(d:{dirType} { Id : {dirId} })")
                        .WithParams(new
                        {
                            elementId = targetId,
                            type= type.ToString(),
                            dirType = ElementTypes.Directory.ToString(),
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
    }
}
