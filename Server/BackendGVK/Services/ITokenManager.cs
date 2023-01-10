using BackendGVK.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BackendGVK.Services
{
    public interface ITokenManager
    {
        Task<string> GenerateTokenAsync(string userId, IEnumerable<Claim> claims, string fingerPrint);
        Task<string> RefreshTokenAsync(string token, string fingerPrint);
        Task<bool> RemoveTokenAsync(string token);
        Task<bool> isActiveTokenAsync(string token);
    }
}
