using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGVK.Models
{
    public class DirectoryModel : Element
    {
        public override ElementTypes Type => ElementTypes.Directory;
    }
}
