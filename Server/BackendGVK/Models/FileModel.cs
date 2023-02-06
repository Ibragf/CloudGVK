using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class FileModel : Element
    {
        public override ElementTypes Type => ElementTypes.File;
        public string TrustedName { get; set; } = null!;
        public string MD5Hash { get; set; } = null!;
        public string Size { get; set; }
    }
}
