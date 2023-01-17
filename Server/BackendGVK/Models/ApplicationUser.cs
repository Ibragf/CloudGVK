using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<AuthToken>? RefreshTokens { get; set;}
        public List<FileModel>? SharedFiles { get; set; }
        public List<DirectoryModel>? SharedDirectories { get; set; }
    }
}
