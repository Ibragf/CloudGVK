using BackendGVK.Models;

namespace BackendGVK.Services.CloudService
{
    public interface ICloud
    {
        Task<IElement> GetElementsAsync(ApplicationUser user, string directory);
        Task ChangeNameAsync(ApplicationUser user, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(ApplicationUser user, string name, ElementTypes type);
        Task AddAsync(ApplicationUser user, IElement element, string directory);
        Task<string> GetPathAsync(ApplicationUser user, string name, ElementTypes type);
        Task MoveToAsync(ApplicationUser user, string name, string directory, ElementTypes type, bool isCopy = false);
        Task SaveChangesAsync();
    }
}
