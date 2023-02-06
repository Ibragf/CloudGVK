using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography;

namespace BackendGVK.Services.SpaceService
{
    public class SpaceManager
    {
        private readonly ICloud _cloudManager;
        public SpaceManager(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        public async Task<FileModel> UploadLargeFiles(HttpContext context, string directory)
        {
            if (context == null || directory == null) throw new ArgumentNullException();

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

                    var file = await IfExistsCreateRelationshipAsync(userId, contentDisposition, directory, data);
                    if(file != null) return file;

                    using (var sha256 = SHA256.Create())
                    {
                        string userId = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value!;
                        string value = userId + untrustedName + DateTime.UtcNow.ToString();
                        trustedName = sha256.ComputeHash();
                    }
                }
            }
        }

        private async Task<FileModel> IfExistsCreateRelationshipAsync(string userId, ContentDispositionHeaderValue contentDisposition, string directory, Dictionary<string, string> dictionary)
        {
            string hashSum = dictionary["md5"];

            var file = await _cloudManager.GetFileByHashSumAsync(hashSum);

            if (file != null)
            {
                string destPath = await _cloudManager.GetPathAsync(userId, directory, ElementTypes.Directory);
                ulong size;
                if (!ulong.TryParse(dictionary["size"], out size)) return null!;

                file.CloudPath = Path.Combine(destPath, contentDisposition.FileName.Value);
                file.Id = Guid.NewGuid().ToString();
                file.UntrustedName = dictionary["name"];
                file.Size = size;
                file.isShared = false;

                bool isAdded = await _cloudManager.AddFileAsync(userId, file, directory);

                if (isAdded) return file;
            }

            return null!;
        }

        private bool FillContentDispositionData(Dictionary<string, string> dictionary, ContentDispositionHeaderValue contentDisposition)
        {
            var nameValues = contentDisposition.Name.Value.Split('.');
            if (nameValues.Length != 2 || nameValues[0] == null || nameValues[1] == null || nameValues[0].Length < 32) return false;
            var untrustedName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

            dictionary.Add("md5",nameValues[0]);
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
