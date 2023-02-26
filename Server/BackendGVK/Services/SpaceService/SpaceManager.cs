using BackendGVK.Controllers;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public class SpaceManager
    {

        private readonly ICloud _cloudManager;
        private readonly FileLoader _fileLoader;
        private readonly HttpContext _context;

        private readonly List<Element> _files;
        private readonly string _boundary;
        private readonly MultipartReader _reader;
        public SpaceManager(ICloud cloudManager, FileLoader fileLoader, HttpContext context)
        {
            _context = context;
            _cloudManager = cloudManager;
            _fileLoader = fileLoader;

            _files = new List<Element>();
            _boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(_context.Request.ContentType), 70);
            _reader = new MultipartReader(_boundary, _context.Request.Body);
        }

        public async Task<IEnumerable<Element>> UploadFiles(string destinationId, string connectionId)
        {
            if (destinationId == null) throw new ArgumentNullException(nameof(destinationId));
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            var section = await _reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                {
                    if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition!)) return null!;

                    string userId = _context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value!;
                    bool isFilled = _fileLoader.FillContentDispositionData(userId, connectionId, contentDisposition!);

                    if (isFilled)
                    {
                        var destPath = await _cloudManager.GetPathAsync(userId, destinationId, ElementTypes.Directory);
                        var inputModel = new CloudInputModel { DestinationId = destinationId, DestinationPath = destPath };

                        var file = await _fileLoader.IfExistsCreateRelationshipAsync(userId, inputModel, _cloudManager);
                        if (file != null)
                        {
                            _files.Add(file);
                            section = await _reader.ReadNextSectionAsync();
                            continue;
                        }

                        file = new FileModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            CloudPath = Path.Combine(destPath, _fileLoader.ContentDispositionData[FileLoader.FILENAME]),
                            Size = _fileLoader.ContentDispositionData[FileLoader.SIZE],
                            CrcHash = _fileLoader.ContentDispositionData[FileLoader.CRCMD5],
                            TrustedName = _fileLoader.TrustedName,
                            UntrustedName = _fileLoader.ContentDispositionData[FileLoader.FILENAME],
                            OwnerId = userId,
                        };

                        var isAdded = await _cloudManager.AddFileAsync(userId, file, inputModel);
                        if (isAdded) _files.Add(file);
                        else
                        {
                            section = await _reader.ReadNextSectionAsync();
                            continue;
                        }

                        inputModel.TargetPath = file.CloudPath;
                        inputModel.TargetId = file.Id;
                        try
                        {
                            bool result = await _fileLoader.ProcessStreamingFileAsync(section, file);
                            if(!result)
                            {
                                await _cloudManager.RemoveAsync(userId, inputModel);
                                continue;
                            }
                            await _cloudManager.Files.Query.Where(nameof(FileModel.Id), file.Id).Update(nameof(FileModel.Size), file.Size).ExecuteAsync();
                        }
                        catch
                        {
                            await _cloudManager.RemoveAsync(userId, inputModel);
                            continue;
                        }
                    }
                    else
                        return _files;

                    section = await _reader.ReadNextSectionAsync();
                }
            }

            return _files;
        }

        public async Task<string> DownloadElementAsync(CloudInputModel cloudInput)
        {
            if(cloudInput.Type == ElementTypes.File)
            {
                var files = await _cloudManager.Files.Query.Where(nameof(FileModel.Id), cloudInput.TargetId).ExecuteAsync();
                var file = files.FirstOrDefault();

                return file?.TrustedName!;
            }

            if(cloudInput.Type == ElementTypes.Directory)
            {
                var dirs = await _cloudManager.Directories.Query.Where(nameof(DirectoryModel.Id), cloudInput.TargetId).ExecuteAsync();
                var dir = dirs.FirstOrDefault();

                FileEntry
            }
        }
    }
}
