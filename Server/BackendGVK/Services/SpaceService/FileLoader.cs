using BackendGVK.Controllers;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public class FileLoader : AbstractLoader
    {
        public const byte SIZE = 0;
        public const byte CRCMD5 = 1;
        public const byte FILENAME = 2;
        public const byte USER = 3;
        public const byte HUB_CONNECTION = 4;

        private readonly IHubContext<ProgressLoadingHub> _hubContext;

        public Dictionary<int, string> ContentDispositionData { get; private set; }

        public FileLoader(IDateProvider dateProvider, IWebHostEnvironment hostEnvironment, IHubContext<ProgressLoadingHub> hubContext)
            : base(dateProvider, hostEnvironment)
        {
            _hubContext = hubContext;
            ContentDispositionData = new Dictionary<int, string>();
        }
        public async Task<FileModel> IfExistsCreateRelationshipAsync(string userId, CloudInputModel cloudInput, ICloud cloudManager)
        {
            string hashSum = ContentDispositionData[CRCMD5];

            var files = await cloudManager.Files.Query.Where(nameof(FileModel.CrcHash), hashSum).ExecuteAsync();
            var file = files.FirstOrDefault();

            if (file != null)
            {
                if (file.Size == ContentDispositionData[SIZE])
                {
                    file.CloudPath = Path.Combine(cloudInput.DestinationPath, ContentDispositionData[FILENAME]);
                    file.Id = Guid.NewGuid().ToString();
                    file.UntrustedName = ContentDispositionData[FILENAME];
                    file.isShared = false;

                    bool isAdded = await cloudManager.AddFileAsync(userId, file, cloudInput);

                    if (isAdded) return file;
                }
            }

            return null!;
        }

        public bool FillContentDispositionData(string userId, string connectionId, ContentDispositionHeaderValue contentDisposition)
        {
            var nameValues = contentDisposition.Name.Value.Split('.');

            if (nameValues.Length != 2 || nameValues[0] == null || nameValues[1] == null || nameValues[0].Length < 10) return false;
            if (userId == null) throw new ArgumentNullException(nameof(userId));
            if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));

            var untrustedName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

            ContentDispositionData.Clear();
            ContentDispositionData.Add(CRCMD5, nameValues[0]);
            ContentDispositionData.Add(SIZE, nameValues[1]);
            ContentDispositionData.Add(FILENAME, untrustedName);
            ContentDispositionData.Add(USER, userId);
            ContentDispositionData.Add(HUB_CONNECTION, connectionId);

            progressChanged += OnProgressChanged;

            return true;
        }

        private async Task OnProgressChanged(int bytesRead)
        {
            string connectionId = ContentDispositionData[HUB_CONNECTION];

            if(bytesRead == 0)
                await _hubContext.Clients.Client(connectionId).SendAsync("closeConnection", connectionId);

            await _hubContext.Clients.Client(connectionId).SendAsync("progressChanged", bytesRead);
        }
    }
}
