using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackendGVK.Services.SpaceService
{
    public class SpaceManager
    {
        private readonly ICloud _cloudManager;
        public SpaceManager(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        public async Task<bool> UploadLargeFiles(HttpContext context)
        {
            
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
