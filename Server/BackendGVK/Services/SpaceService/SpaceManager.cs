using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services.SpaceService
{
    public class SpaceManager
    {
        private const string SIZE = "size";
        private const string CRCMD5 = "CrcHash";
        private const string FILENAME = "filename";

        private readonly ICloud _cloudManager;
        public SpaceManager(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        public async Task<IEnumerable<Element>> UploadLargeFiles(HttpContext context, string destinationPath)
        {
            List<Element> files = new List<Element>();
            if (context == null || destinationPath == null) throw new ArgumentNullException();

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(context.Request.ContentType), 70);
            var reader = new MultipartReader(boundary, context.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            Dictionary<string, string> data = new Dictionary<string, string>(3);
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                {
                    if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition!)) return null!;

                    string userId = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value!;
                    FillContentDispositionData(data, contentDisposition!);

                    var file = await IfExistsCreateRelationshipAsync(userId, contentDisposition!, destinationPath, data);
                    if (file != null)
                    {
                        files.Add(file);
                        section = await reader.ReadNextSectionAsync();
                        continue;
                    }

                    string trustedName = GenerateTrustedName(userId, data);
                    file = new FileModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        CloudPath = Path.Combine(destinationPath, data[FILENAME]),
                        Size = data[SIZE],
                        CrcHash = data[CRCMD5],
                        TrustedName = trustedName,
                        UntrustedName = data[FILENAME],
                        OwnerId = userId,
                    };

                    var isAdded = await _cloudManager.AddFileAsync(userId, file, destinationPath);
                    if (isAdded) files.Add(file);
                    else
                    {
                        section = await reader.ReadNextSectionAsync();
                        continue;
                    }

                    bool result = await FilesHelper.ProcessStreamingFileAsync(section, file.TrustedName);
                    if (!result)
                    {
                        await _cloudManager.RemoveAsync(userId, file.Id, file.Type);
                        continue;
                    }
                    /*try
                    {
                        bool result = await FilesHelper.ProcessStreamingFileAsync(section, file.TrustedName);
                        if(!result)
                        {
                            await _cloudManager.RemoveAsync(userId, file.Id, file.Type);
                            continue;
                        }
                    }
                    catch
                    {
                        await _cloudManager.RemoveAsync(userId, file.Id, file.Type);
                        continue;
                    }*/

                    section = await reader.ReadNextSectionAsync();
                }
            }

            return files;
        }

        private string GenerateTrustedName(string userId, Dictionary<string, string> data)
        {
            string trustedName;
            using (var sha256 = SHA256.Create())
            {
                string value = userId + data[FILENAME] + DateTime.UtcNow.ToString() + data[SIZE];
                var buffer = Encoding.UTF8.GetBytes(value);
                var hash = sha256.ComputeHash(buffer, 0, buffer.Length);
                StringBuilder sb = new StringBuilder();
                foreach (var byt in hash)
                {
                    sb.Append(byt.ToString("x2"));
                }
                trustedName = sb.ToString();
            }

            return trustedName;
        }
        private async Task<FileModel> IfExistsCreateRelationshipAsync(string userId, ContentDispositionHeaderValue contentDisposition, string directoryPath, Dictionary<string, string> dictionary)
        {
            string hashSum = dictionary[CRCMD5];

            var files = await _cloudManager.Files.Query.Where(nameof(FileModel.CrcHash), hashSum).ExecuteAsync();
            var file = files.FirstOrDefault();

            if (file != null)
            {
                string destPath = await _cloudManager.GetPathAsync(userId, directoryPath, ElementTypes.Directory);

                if (file.Size == dictionary[SIZE])
                {
                    file.CloudPath = Path.Combine(destPath, contentDisposition.FileName.Value);
                    file.Id = Guid.NewGuid().ToString();
                    file.UntrustedName = dictionary[FILENAME];
                    file.isShared = false;

                    bool isAdded = await _cloudManager.AddFileAsync(userId, file, directoryPath);

                    if (isAdded) return file;
                }
            }

            return null!;
        }
        private bool FillContentDispositionData(Dictionary<string, string> dictionary, ContentDispositionHeaderValue contentDisposition)
        {
            var nameValues = contentDisposition.Name.Value.Split('.');
            if (nameValues.Length != 2 || nameValues[0] == null || nameValues[1] == null || nameValues[0].Length < 10) return false;
            var untrustedName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

            dictionary.Clear();
            dictionary.Add(CRCMD5, nameValues[0]);
            dictionary.Add(SIZE, nameValues[1]);
            dictionary.Add(FILENAME, untrustedName);

            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var factories = context.ValueProviderFactories;
            factories.RemoveType<FormValueProviderFactory>();
            factories.RemoveType<FormFileValueProviderFactory>();
            factories.RemoveType<JQueryFormValueProviderFactory>();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
