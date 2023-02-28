using BackendGVK.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public abstract class AbstractLoader
    {
        protected event ProgressChangedHandler? ProgressChanged;

        protected readonly IDateProvider _dateProvider;
        protected readonly IWebHostEnvironment _hostEnvironment;

        protected delegate void ProgressChangedHandler(int bytesRead);

        public AbstractLoader(IDateProvider dateProvider, IWebHostEnvironment hostEnvironment) 
        {
            _dateProvider = dateProvider;
            _hostEnvironment = hostEnvironment;
        }

        public virtual string CreateTrustedName(params string[] values)
        {
            string result;
            StringBuilder sb = new StringBuilder();
            foreach (var value in values)
            {
                sb.Append(value);
            }
            sb.Append(_dateProvider.GetCurrentDate().ToString());

            var buffer = Encoding.UTF8.GetBytes(sb.ToString());
            using(var sha256 = SHA256.Create())
            {
                sb.Clear();
                var hashBytes = sha256.ComputeHash(buffer);
                foreach (var item in hashBytes)
                {
                    sb.Append(item.ToString("x2"));
                }
                result = sb.ToString();
            }

            return result;
        }

        public virtual string CreateFilePath(string trustedName)
        {
            string firstPart = Path.Combine(_hostEnvironment.ContentRootPath, trustedName.Substring(0, 3));
            string secondPart = Path.Combine(firstPart, trustedName.Substring(3, 3));
            string thirdPart = trustedName.Substring(6, trustedName.Length - 6);

            if (!Directory.Exists(firstPart))
                Directory.CreateDirectory(firstPart);
            if (!Directory.Exists(secondPart))
                Directory.CreateDirectory(secondPart);

            string path = Path.Combine(secondPart, thirdPart);

            if (File.Exists(path))
                return null!;
            else 
                return path;
        }

        public async Task<bool> ProcessRecordingFileAsync(Stream sourceStream, string filePath)
        {
            const int chunkSize = 1024 * 1024; // 1МБ
            var buffer = new byte[chunkSize];
            int bytesRead = 0;

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                do
                {
                    bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);

                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                    if (ProgressChanged != null)
                        ProgressChanged.Invoke(bytesRead);

                } while (bytesRead > 0);
            }

            if (File.Exists(filePath)) return true;

            return false;
        }
    }
}
