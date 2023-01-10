using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class AuthToken
    {
        [Key]
        [MaxLength(256)]
        public string Id { get; set; }
        public DateTime Exp { get; set; }

        [MaxLength(500)]
        public string Token { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
