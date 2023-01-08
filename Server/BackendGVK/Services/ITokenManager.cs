using System.Security.Claims;

namespace BackendGVK.Services
{
    public interface ITokenManager
    {
        Task<bool> isActiveAsync(string token);
        Task<bool> DeactiveTokensAsync(string access, string refresh);
        string GenerateToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
    }
}
