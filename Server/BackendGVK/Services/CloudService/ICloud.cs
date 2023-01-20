using BackendGVK.Models;

namespace BackendGVK.Services.CloudService
{
    public interface ICloud
    {
        Task<IEnumerable<Element>> GetElementsAsync(ApplicationUser user, string directory);
        Task<bool> ChangeNameAsync(ApplicationUser user, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(ApplicationUser user, string name, ElementTypes type);
        Task<bool> AddAsync(ApplicationUser user, Element element, string directory);
        Task<string> GetPathAsync(ApplicationUser user, string name, ElementTypes type);
        Task MoveToAsync(ApplicationUser user, string name, string directoryDestination, ElementTypes type);
        Task CopyToAsync(ApplicationUser user, string name, string destination, ElementTypes type);
        Task MoveToAccessModeAsync(ApplicationUser user, string name, string destination, ElementTypes type)
        Task SaveChangesAsync();
    }
}
