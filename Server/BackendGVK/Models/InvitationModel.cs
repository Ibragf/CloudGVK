using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Models
{
    public class InvitationModel
    {
        public string Id { get; set; }
        public string DirectoryId { get; set; }
        public string From { get; set; }
        public string Directory { get; set; }
        public string Size { get; set; }
    }
}
