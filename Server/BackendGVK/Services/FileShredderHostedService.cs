using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Neo4jClient;
using Newtonsoft.Json.Linq;

namespace BackendGVK.Services
{
    public class FileShredderHostedService : BackgroundService
    {
        private readonly IGraphClient _clientGraph;
        private readonly IDateProvider _dateProvider;
        private List<Element> _elements;
        private const int MAX_PAUSE = 1000 * 60 * 60 * 60;
        public FileShredderHostedService(IGraphClient clientGraph, IDateProvider dateProvider)
        {
            _clientGraph = clientGraph;
            _elements = new List<Element>();
            _dateProvider = dateProvider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pause = 0;
            while(!stoppingToken.IsCancellationRequested)
            {
                await GetElementsAsync();

                if (_elements.Count == 50)
                {
                    pause -= 1000 * 60 * 5;
                    if (pause < 0) pause = 0;
                }
                else
                {
                    pause += 1000 * 60 * 5;
                    if (pause > MAX_PAUSE) pause = MAX_PAUSE;
                }

                foreach (var item in _elements)
                {
                    if (item is FileModel file)
                    {
                        var count = await GetFileCount(file);
                        if (count == 1)
                        {
                            if (File.Exists(file.TrustedName))
                            {
                                File.Delete(file.TrustedName);
                            }
                        }

                        await DeleteElementAsync(file);
                        _elements.Remove(item);
                    }
                }

                foreach (var item in _elements)
                {
                    if (item is DirectoryModel directory)
                    {
                        var files = await GetAllFilesAsync(directory);
                        foreach (var file in files)
                        {
                            var count = await GetFileCount(file);
                            if (count == 1)
                            {
                                if (File.Exists(file.TrustedName))
                                {
                                    File.Delete(file.TrustedName);
                                }
                            }

                            await DeleteElementAsync(file);
                            _elements.Remove(item);
                        }

                        await DeleteElementAsync(directory);
                        _elements.Remove(item);
                    }
                }

                if (pause == 0) continue;
                else await Task.Delay(pause);
            }
        }

        private async Task GetElementsAsync()
        {
            var ticks = _dateProvider.GetCurrentDate().Ticks;

            var filesTask = _clientGraph.Cypher
                .Match($"(f:{ElementTypes.File})")
                .Where("f.DeleteDate < $ticks")
                .WithParam("ticks", ticks)
                .Limit(40)
                .Return(f => f.As<FileModel>())
                .ResultsAsync;

            var dirsTask = _clientGraph.Cypher
                .Match($"(d:{ElementTypes.Directory})")
                .Where("d.DeleteDate < $ticks")
                .WithParam("ticks", ticks)
                .Limit(10)
                .Return(d => d.As<DirectoryModel>())
                .ResultsAsync;

            var files = await filesTask;
            var dirs = await dirsTask;

            _elements.AddRange(files);
            _elements.AddRange(dirs);
        }

        private async Task<IEnumerable<FileModel>> GetAllFilesAsync(DirectoryModel directory)
        {
            var files = await _clientGraph.Cypher
                .Match($"(:{directory.Type} {{ Id : $id }})-[:HAS*]->(f)")
                .WithParam("id", directory.Id)
                .Return(f => f.As<FileModel>())
                .ResultsAsync;

            return files;
        }

        private async Task DeleteElementAsync(Element element)
        {
            await _clientGraph.Cypher
                .Match($"(e:{element.Type} {{Id : $id }})")
                .DetachDelete("e")
                .WithParam("id", element.Id)
                .ExecuteWithoutResultsAsync();
        }

        private async Task<long> GetFileCount(FileModel file)
        {
            var response = await _clientGraph.Cypher
                .Match($"(f:{file.Type} {{ TrustedName : $trustedName }})")
                .WithParam("trustedName", file.TrustedName)
                .Return(f => f.Count())
                .ResultsAsync;

            return response.FirstOrDefault()!;
        }
    }
}
