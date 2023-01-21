using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authorization;

namespace BackendGVK.Policy.isAllowed
{
    public class HasAccessHandler : AuthorizationHandler<isAllowedRequirement, Element>
    {
        private readonly ICloud _cloudManager;
        public HasAccessHandler(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, isAllowedRequirement requirement, Element resource)
        {
            string id = context.User.Claims.FirstOrDefault()?.Value!;
            if (id == null) return;

            bool hasAccess = await _cloudManager.HasAccessAsync(id, resource.Id, resource.Type);

            if (hasAccess) context.Succeed(requirement);
        }
    }
}
