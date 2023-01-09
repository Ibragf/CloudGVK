using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey("TokenId")]
        public RefreshToken? RefreshToken { get; set; }
        public string? TokenId { get; set; }
    }
}
