using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class FileModel : IElement
    {
        [Key]
        public Guid Id { get; set; }

        [NotMapped]
        public ElementTypes Type => ElementTypes.File;

        [MaxLength(50)]
        public string UntrustedName { get; set; } = null!;
        public string TrustedName { get; set; } = null!;
        public ulong Size { get; set; }
        public string MD5Hash { get; set; } = null!;
        public Guid DirectoryId { get; set; }
        public string OwnerId { get; set; } = null!;
        public List<ApplicationUser>? AllowedUsers { get; set; }
    }
}
