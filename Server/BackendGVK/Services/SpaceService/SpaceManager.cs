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
        private readonly ICloud _cloudManager;
        public SpaceManager(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        public async Task<IEnumerable<FileModel>> UploadLargeFiles(HttpContext context, string directoryId)
        {
            List<FileModel> files = new List<FileModel>();
            if (context == null || directoryId == null) throw new ArgumentNullException();

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

                    var file = await IfExistsCreateRelationshipAsync(userId, contentDisposition!, directoryId, data);
                    if (file != null)
                    {
                        files.Add(file);
                        continue;
                    }

                    string trustedName = GenerateTrustedName(userId, data);
                    string destPath = await _cloudManager.GetPathAsync(userId, directoryId, ElementTypes.Directory);
                    file = new FileModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        CloudPath = Path.Combine(destPath, data["name"]),
                        Size = data["size"],
                        MD5Hash = data["md5"],
                        TrustedName = trustedName,
                        UntrustedName = data["name"],
                    };

                    var isAdded = await _cloudManager.AddFileAsync(userId, file, directoryId);
                    if (isAdded) files.Add(file);
                    else continue;//!!!!!!!!!!!!!!

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
                string value = userId + data["name"] + DateTime.UtcNow.ToString() + data["size"];
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
        private async Task<FileModel> IfExistsCreateRelationshipAsync(string userId, ContentDispositionHeaderValue contentDisposition, string directoryId, Dictionary<string, string> dictionary)
        {
            string hashSum = dictionary["md5"];

            var files = await _cloudManager.Files.Query.Where(nameof(FileModel.MD5Hash), hashSum).ExecuteAsync();
            var file = files.FirstOrDefault();

            if (file != null)
            {
                string destPath = await _cloudManager.GetPathAsync(userId, directoryId, ElementTypes.Directory);

                if (file.Size == dictionary["size"])
                {
                    file.CloudPath = Path.Combine(destPath, contentDisposition.FileName.Value);
                    file.Id = Guid.NewGuid().ToString();
                    file.UntrustedName = dictionary["name"];
                    file.isShared = false;

                    bool isAdded = await _cloudManager.AddFileAsync(userId, file, directoryId);

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
            dictionary.Add("md5", nameValues[0]);
            dictionary.Add("size", nameValues[1]);
            dictionary.Add("name", untrustedName);

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
