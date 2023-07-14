using BackendGVK.Controllers;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using System.Security.Claims;

namespace BackendGVK.Extensions
{
    public static class AuthHelper
    {
        public static string GetUserId(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(x => x.Type == "Id");
            if (claim == null) return null!;
            return claim.Value;
        }

        public static async Task<bool> isAllowedAsync(string userId, CloudInputModel model, ICloud cloudManager)
        {
            bool hasAccess = await cloudManager.HasAccessAsync(userId, model);
            bool isOwner = await cloudManager.isOwnerAsync(userId, model);

            return hasAccess || isOwner;
        }
    }
}
