using BackendGVK.Db;
using BackendGVK.Models;
using BackendGVK.Services.Configs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly JwtSettings _jwtSettings;
        private readonly IDistributedCache _cache;
        private readonly AppDbContext _dbContext;
        public TokenManager(IDistributedCache cache, IOptions<JwtSettings> jwtSettings, AppDbContext context)
        {
            _cache = cache;
            _jwtSettings = jwtSettings.Value;
            _dbContext = context;
        }
        public async Task<string> GenerateTokenAsync(string userId, IEnumerable<Claim> claims, string fingerPrint)
        {
            DateTime dateTime;
            string jwt = GenerateToken(claims, out dateTime);
            string email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            string hashFinger = GetHash256(fingerPrint + email);

            var result = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.Id == hashFinger);

            if(result != null) {
                bool isDeactive = await DeactiveTokenAsync(result.Token);
                if (isDeactive)
                {
                    result.Token = jwt;
                    result.Exp = dateTime;
                }
                else return null;
            }
            else
            {
                AuthToken token = new AuthToken
                {
                    Id = hashFinger,
                    Exp = dateTime,
                    Token = jwt,
                    ApplicationUserId = userId,
                };
                await _dbContext.Tokens.AddAsync(token);
            }

            await _dbContext.SaveChangesAsync();

            return jwt;
        }
        public async Task<string> RefreshTokenAsync(string token, string fingerPrint)
        {
            var principal = GetClaimsPrincipal(token);
            if (principal == null) return null;

            string email = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            string hashFinger = GetHash256(fingerPrint + email);
            AuthToken? authToken = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.Id == hashFinger);
            if (authToken == null || authToken.Token!= token) return null;

            bool isDeactivated = await DeactiveTokenAsync(token);
            if (!isDeactivated) return null;

            DateTime dateTime;
            string jwt = GenerateToken(principal.Claims, out dateTime);

            AuthToken refreshToken = new AuthToken
            {
                Id = authToken.Id,
                Token = jwt,
                Exp = dateTime,
                ApplicationUserId = authToken.ApplicationUserId
            };

            _dbContext.Tokens.Remove(authToken);
            await _dbContext.Tokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            return jwt;
        }
        public async Task<bool> RemoveTokenAsync(string token)
        {
            bool isDeactive = await DeactiveTokenAsync(token);
            if(!isDeactive) return false;

            AuthToken authToken = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.Token==token);
            if (authToken != null)
            {
                _dbContext.Remove(authToken);
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }
        public async Task<bool> isActiveTokenAsync(string token)
        {
            string result = await _cache.GetStringAsync($"tokens:{token}");
            return result == null ? true : false;
        }

        private async Task<bool> DeactiveTokenAsync(string token)
        {
            var principal = GetClaimsPrincipal(token);
            if (principal == null) return false;
            string utcValue = principal.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
            if (utcValue == null) return false;

            double utc;
            try
            {
                utc = Double.Parse(utcValue);
            }
            catch (Exception ex) { return false; }

            DateTime datetime = UnixTimeStampToDateTime(utc);
            TimeSpan exp = datetime.Subtract(DateTime.UtcNow).Add(new TimeSpan(0, 3, 0));
            await _cache.SetStringAsync($"tokens:{token}", "deactivated", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = exp });

            return true;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
        private ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        private string GenerateToken(IEnumerable<Claim> claims, out DateTime datetime)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            datetime = DateTime.UtcNow.AddMinutes(15);

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                signingCredentials: new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256),
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: datetime
            );

            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }
        private string GetHash256(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(bytes);
            }

            StringBuilder sb = new StringBuilder();
            foreach (var byt in hashBytes)
            {
                sb.Append(byt.ToString("x2"));
            }
            string hashString = sb.ToString();
            return hashString;
        }
    }
}
