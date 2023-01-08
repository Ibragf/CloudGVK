using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Models
{
    public class TokensModel
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
