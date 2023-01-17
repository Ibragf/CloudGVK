using BackendGVK.Services.TokenManagerService;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace BackendGVK.Extensions
{
    public class TokenManagerMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenManagerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenManager tokenManager)
        {
            var authHeader = context.Request.Headers.Authorization;

            string token = authHeader == StringValues.Empty ? string.Empty : authHeader.Single().Split(' ').Last();
            bool isActive = await tokenManager.isActiveTokenAsync(token);

            if (isActive) await _next.Invoke(context);
            else context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }
    }
}
