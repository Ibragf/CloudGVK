using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Models
{
    public class Element
    {
        public string Id { get; set; } = null!;
        public virtual ElementTypes Type { get; }
        public string CloudPath { get; set; } = null!;
        public bool isAdded { get; set; }
        public string UntrustedName { get; set; } = null!;
        public bool isShared { get; set; }
        public DateTime? DeleteDate { get; set; }
    }

    public enum ElementTypes
    {
        File,
        Directory
    }
}
