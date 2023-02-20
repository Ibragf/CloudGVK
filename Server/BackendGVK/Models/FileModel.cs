using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using newtonsoft = Newtonsoft.Json;

namespace BackendGVK.Models
{
    public class FileModel : Element
    {
        public override ElementTypes Type => ElementTypes.File;

        [JsonIgnore]
        [newtonsoft.JsonIgnore]
        public string TrustedName { get; set; } = null!;

        [JsonIgnore]
        [newtonsoft.JsonIgnore]
        public string CrcHash { get; set; } = null!;
    }
}
