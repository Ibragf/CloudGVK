using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class AuthToken
    {
        [Key]
        [MaxLength(250)]
        public string Id { get; set; }
        public DateTime Exp { get; set; }

        [MaxLength(100)]
        public string FingerPrint { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
