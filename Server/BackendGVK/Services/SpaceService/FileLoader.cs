using BackendGVK.Controllers;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public class FileLoader
    {
        public const byte SIZE = 0;
        public const byte CRCMD5 = 1;
        public const byte FILENAME = 2;
        public const byte USER = 3;
        public const byte HUB_CONNECTION = 4;
        public const string DISK = @"G:\";

        private string _trustedName = null!;
        private string _filePath = null!;
        private readonly IDateProvider _dateProvider;

        public string FilePath
        {
            get
            {
                while (_filePath == null)
                    _filePath = CreatePathFromTrustedName();

                return _filePath;
            }
            private set { _filePath = value; }
        }
        public string TrustedName
        {
            get
            {
                if (_trustedName == null)
                    _trustedName = CreateTrustedName();

                return _trustedName;
            }
            private set { _trustedName = value; }
        }
        public Dictionary<int, string> ContentDispositionData { get; private set; }

        public FileLoader(IDateProvider dateProvider)
        {
            _dateProvider = dateProvider;
            ContentDispositionData = new Dictionary<int, string>();
        }
        public async Task<bool> ProcessStreamingFileAsync(MultipartSection section, FileModel file)
        {
            ulong size = 0;
            const int chunkSize = 1024 * 1024; // 1МБ
            var buffer = new byte[chunkSize];
            int bytesRead = 0;

            using (var stream = new FileStream(FilePath, FileMode.Create))
            {
                do
                {
                    bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    size += (ulong) bytesRead;

                } while (bytesRead > 0);
            }

            file.Size = size.ToString();
            ContentDispositionData.Clear();
            if (File.Exists(FilePath)) return true;

            return false;
        }

        public string CreatePathFromTrustedName()
        {
            string firstPart = Path.Combine(DISK, TrustedName.Substring(0, 3));
            string secondPart = Path.Combine(firstPart, TrustedName.Substring(3, 3));
            string thirdPart = TrustedName.Substring(6, TrustedName.Length - 6);

            if (Directory.Exists(firstPart))
            {
                TrustedName = null!;
                return null!;
            }
            Directory.CreateDirectory(firstPart);
            if (Directory.Exists(secondPart))
            {
                TrustedName = null!;
                return null!;
            }
            Directory.CreateDirectory(secondPart);

            FilePath = Path.Combine(secondPart, thirdPart);
            return FilePath;
        }

        public string CreateTrustedName()
        {
            if (TrustedName != null) return TrustedName;

            if (ContentDispositionData.Count == 0) throw new InvalidOperationException("ContentDispositionData is empty.");
            string userId = ContentDispositionData[USER];
            string filename = ContentDispositionData[FILENAME];
            string size = ContentDispositionData[SIZE];

            if (userId == null) throw new NullReferenceException("Value of ContentDispositionData with key 'userId' is null.");
            if (filename == null) throw new NullReferenceException("Value of ContentDispositionData with key 'filename' is null.");
            if (size == null) throw new NullReferenceException("Value of ContentDispositionData with key 'size' is null.");

            string trustedName;
            using (var sha256 = SHA256.Create())
            {
                string value = userId + filename + _dateProvider.GetCurrentDate() + size;
                var buffer = Encoding.UTF8.GetBytes(value);
                var hash = sha256.ComputeHash(buffer, 0, buffer.Length);
                StringBuilder sb = new StringBuilder();
                foreach (var byt in hash)
                {
                    sb.Append(byt.ToString("x2"));
                }
                trustedName = sb.ToString();
            }

            TrustedName = trustedName;
            return trustedName;
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

            return true;
        }
    }
}
