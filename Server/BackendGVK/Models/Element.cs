using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using newtonsoft = Newtonsoft.Json;

namespace BackendGVK.Models
{
    public class Element
    {
        public string Id { get; set; } = null!;
        public virtual ElementTypes Type { get; }
        public string CloudPath { get; set; } = null!;
        public string UntrustedName { get; set; } = null!;
        public bool isShared { get; set; }
        public string OwnerId { get; set; } = null!;
        public string? Size { get; set; }

        [JsonIgnore]
        [newtonsoft.JsonIgnore]
        public long DeleteDate { get; set; }

        [JsonIgnore]
        [newtonsoft.JsonIgnore]
        public bool isAdded { get; set; }
    }

    public enum ElementTypes
    {
        File,
        Directory
    }
}
