using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.SignalR;
using Neo4jClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public class FileArchiver : AbstractLoader
    {
        private readonly IGraphClient _clientGraph;
        private readonly ICloud _cloudManager;
        private readonly IHubContext<ProgressLoadingHub> _hubContext;
        public string ZipPath { get; private set; } = null!;
        public FileArchiver(IDateProvider dateProvider, IWebHostEnvironment hostEnvironment, IGraphClient clientGraph, ICloud cloudManager, IHubContext<ProgressLoadingHub> hubContext)
            : base(dateProvider, hostEnvironment)
        {
            _clientGraph = clientGraph;
            _cloudManager = cloudManager;
            _hubContext = hubContext;
        }

        public async Task CreateTempZipFileAsync(string userId, string zipPath, string connectionId, DirectoryModel directory)
        {
            if (userId == null) throw new ArgumentNullException(nameof(userId));
            if (zipPath == null) throw new ArgumentNullException(nameof(zipPath));
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            int startIndex = directory.CloudPath.Length + 2;

            Stack<DirectoryModel> stack = new Stack<DirectoryModel>();
            stack.Push(directory);

            CloudInputModel cloudInput = new CloudInputModel();
            
            using (var zipStream = new FileStream(zipPath, FileMode.Create))
            {
                var zipFile = ZipFile.Create(zipStream);
                var zipFactory = new ZipEntryFactory();
                zipFile.BeginUpdate();

                while (stack.Count > 0)
                {
                    var dir = stack.Pop();
                    cloudInput.TargetId = dir.Id;
                    cloudInput.TargetPath = dir.CloudPath;

                    var internalElements = await _cloudManager.GetElementsAsync(userId, cloudInput);

                    foreach (var file in internalElements.Files)
                    {
                        string path = file.CloudPath.Substring(startIndex, file.CloudPath.Length - startIndex);

                        var dataSource = new StaticDiskDataSource(file.TrustedName);
                        var fileEntry = zipFactory.MakeFileEntry(path);

                        zipFile.Add(dataSource, fileEntry);
                        await _hubContext.Clients.Client(connectionId).SendAsync("progressArchivingChanged", zipStream.Length);
                    }
                    foreach(var internalDir in internalElements.Directories)
                    {
                        stack.Push(internalDir);

                        string path = internalDir.CloudPath.Substring(startIndex, internalDir.CloudPath.Length - startIndex);

                        var dirEntry = zipFactory.MakeDirectoryEntry(path);
                        zipFile.Add(dirEntry);
                    }
                }

                zipFile.CommitUpdate();
                zipFile.Close();
                await _hubContext.Clients.Client(connectionId).SendAsync("closeConnection", connectionId);

                var task = DeleteZipFileAsync(zipPath);
            }
        }
        
        private async Task DeleteZipFileAsync(string trustedName)
        {
            var file = new FileModel
            {
                Id = Guid.NewGuid().ToString(),
                TrustedName = trustedName,
                DeleteDate = _dateProvider.GetCurrentDate().AddDays(1).Ticks,
            };

            await _clientGraph.Cypher
                .Merge($"(f:{ElementTypes.File} {{ TrustedName = $trustedName }})")
                .OnCreate()
                .Set("f = $instance")
                .WithParams(new
                {
                    trustedName,
                    instance = file,
                })
                .ExecuteWithoutResultsAsync();
        }
    }
}
