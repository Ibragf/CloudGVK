namespace BackendGVK.Models
{
    public abstract class Element
    {
        public virtual ElementTypes Type { get; }
        public List<ApplicationUser>? AllowedUsers { get; set; }
        public string OwnerId { get; set; } = null!;
        public string Path { get; set; } = null!;
        public ulong Size { get; set; }
    }

    public enum ElementTypes
    {
        File,
        Directory
    }
}
