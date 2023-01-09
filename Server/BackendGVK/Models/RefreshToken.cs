using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public DateTime exp { get; set; }
    }
}
