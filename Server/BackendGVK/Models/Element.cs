namespace BackendGVK.Models
{
    public abstract class Element
    {
        public string Id { get; set; } = null!;
        public virtual ElementTypes Type { get; }
        public string Path { get; set; } = null!;
        public ulong Size { get; set; }
        public bool isAdded { get; set; }
        public string UntrustedName { get; set; } = null!;
        public bool isShared { get; set; }
    }

    public enum ElementTypes
    {
        File,
        Directory
    }
}
