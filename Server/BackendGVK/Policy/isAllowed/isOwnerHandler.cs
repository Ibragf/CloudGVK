﻿using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authorization;

namespace BackendGVK.Policy.isAllowed
{
    public class isOwnerHandler : AuthorizationHandler<isAllowedRequirement, Element>
    {
        private readonly ICloud _cloudManager;
        public isOwnerHandler(ICloud cloudManager)
        {
            _cloudManager = cloudManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, isAllowedRequirement requirement, Element resource)
        {
            string id = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value!;
            if (id == null) return;

            bool isOwner = await _cloudManager.isOwnerAsync(id, resource.Id, resource.Type);

            if (isOwner) context.Succeed(requirement);
        }
    }
}
