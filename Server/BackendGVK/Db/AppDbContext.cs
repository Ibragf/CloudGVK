using BackendGVK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendGVK.Db
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<AuthToken> Tokens { get; set; }
        public DbSet<DirectoryModel> Directories { get; set; }
        public DbSet<FileModel> Files { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

    }
}
