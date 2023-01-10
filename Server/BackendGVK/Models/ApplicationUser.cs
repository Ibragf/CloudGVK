using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<AuthToken>? RefreshTokens { get; set;}
    }
}
