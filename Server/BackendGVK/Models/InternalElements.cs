namespace BackendGVK.Models
{
    public class InternalElements
    {
        public IEnumerable<FileModel> Files { get; set; }
        public IEnumerable<DirectoryModel> Directories { get; set; }
        public IEnumerable<DirectoryModel> Shared { get; set; }
    }
}
