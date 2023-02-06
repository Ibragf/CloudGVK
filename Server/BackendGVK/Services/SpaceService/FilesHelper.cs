using Microsoft.AspNetCore.WebUtilities;

namespace BackendGVK.Services.SpaceService
{
    public static class FilesHelper
    {
        public static string DISK = @"G:\";
        public static async Task<bool> ProcessStreamingFileAsync(MultipartSection section, string trustedName)
        {
            string path = CreatePathFromTrustedName(trustedName);
            if(path == null) return false;

            const int chunkSize = 1024*1024; // 1МБ
            var buffer = new byte[chunkSize];
            int bytesRead = 0;

            using (var stream = new FileStream(path, FileMode.Create))
            {
                do
                {
                    bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                    await stream.WriteAsync(buffer, 0, bytesRead);

                } while (bytesRead > 0);
            }

            if (File.Exists(path)) return true;

            return false;
        }

        public static string GetPathFromTrustedName(string trustedName)
        {
            string firstPart = trustedName.Substring(0, 3);
            string secondPart = trustedName.Substring(3, 3);
            string thirdPart = trustedName.Substring(6, trustedName.Length - 6);

            return Path.Combine(DISK,firstPart, secondPart, thirdPart);
        }

        public static string CreatePathFromTrustedName(string trustedName)
        {
            string firstPart = Path.Combine(DISK, trustedName.Substring(0, 3));
            string secondPart = Path.Combine(firstPart, trustedName.Substring(3, 3));
            string thirdPart = trustedName.Substring(6, trustedName.Length - 6);

            if (Directory.Exists(firstPart)) return null!;
            Directory.CreateDirectory(firstPart);
            if (Directory.Exists(secondPart)) return null!;
            Directory.CreateDirectory(secondPart);

            return Path.Combine(secondPart, thirdPart);
        }
    }
}
