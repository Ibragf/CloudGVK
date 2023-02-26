using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Models
{
    public class CloudInputModel
    {
        public string TargetId { get; set; } = null!;
        public string DestinationId { get; set; } = null!;
        public string TargetPath { get; set; } = null!;
        public string DestinationPath { get; set; } = null!;
        [Required]
        public ElementTypes Type { get; set; }
    }
}
