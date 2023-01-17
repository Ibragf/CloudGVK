using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class DirectoryModel : IElement
    {
        [Key]
        public Guid Id { get; set; }

        [NotMapped]
        public ElementTypes Type { get => ElementTypes.Directory; }

        [MaxLength(50)]
        public string Name { get; set; } = null!;
        public ulong Size { get; set; }
        public string OwnerId { get; set; } = null!;
        public Guid? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public DirectoryModel? Parent { get; set; }
        public List<FileModel>? Files { get; set; }
        public List<DirectoryModel>? Directories { get; set; }
        public List<ApplicationUser>? AllowedUsers { get ; set ; }
    }
}
