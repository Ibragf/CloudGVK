using BackendGVK.Services.Configs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BackendGVK.Services
{
    public class TokenManager : ITokenManager
    {
        private JwtSettings _jwtSettings;
        private IDistributedCache _cache;
        public TokenManager(IDistributedCache cache, IOptions<JwtSettings> jwtSettings)
        {
            _cache = cache;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<bool> DeactiveTokensAsync(string access, string refresh, string expiration)
        {
            var utcValue = expiration;
            if (utcValue == null) return false;

            double utc;
            try
            {
                utc = Double.Parse(utcValue);
            }
            catch(Exception ex) { return false; }

            DateTime datetime = UnixTimeStampToDateTime(utc);
            TimeSpan exp=datetime.Subtract(DateTime.UtcNow.AddMinutes(3));
            await _cache.SetStringAsync($"tokens:{refresh}:deactivated",string.Empty, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = exp} );
            await _cache.SetStringAsync($"tokens:{access}:deactivated", string.Empty, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = exp });
            return true;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                signingCredentials: new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256),
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(10)
            );

            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }

        public async Task<bool> isActiveAsync(string token)
        {
            string result = await _cache.GetStringAsync(token);
            return result == null ? true : false;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}
