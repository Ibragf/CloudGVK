using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;

namespace BackendGVK.Services
{
    public class ConfirmTokenProvider : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        private IReadOnlyList<string> values = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private readonly AppDbContext _dbContext;
        public ConfirmTokenProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return true;
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            if (user == null || manager == null) return null;
            string token = string.Empty;
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                sb.Append(values[random.Next(0, 10)]);
            }
            token = sb.ToString();

            var userToken = new IdentityUserToken<string>
            {
                LoginProvider = this.ToString(),
                UserId = user.Id,
                Name = purpose,
                Value = token
            };

            var userTokens = await _dbContext.UserTokens.Where(x => x.UserId==user.Id).ToListAsync();
            if(userTokens.Count>0)
            {
                _dbContext.UserTokens.RemoveRange(userTokens);
            }
            await _dbContext.UserTokens.AddAsync(userToken);
            await _dbContext.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var result = await _dbContext.UserTokens.FirstOrDefaultAsync(x => x.Value == token && x.Name == purpose && x.UserId == user.Id);
            if (result == null) return false;
            else
            {
                _dbContext.UserTokens.Remove(result);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
    }
}
