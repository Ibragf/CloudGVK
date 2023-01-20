using BackendGVK.Controllers;
using BackendGVK.Models;

namespace BackendGVK.Services.CloudService
{
    public interface ICloud
    {
        Task<OutputElements> GetElementsAsync(string userId, string directory);
        Task<bool> ChangeNameAsync(string userId, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(string userId, string name, ElementTypes type);
        Task<bool> AddFileAsync(string userId, FileModel file, string directory);
        Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string directory);
        Task<string> GetPathAsync(string userId, string name, ElementTypes type);
        Task MoveToAsync(string userId, string name, string directoryDestination, ElementTypes type);
        Task CopyToAsync(string userId, string name, string destination, ElementTypes type);
        Task MoveToAccessModeAsync(string userId, string name, string destination, ElementTypes type);
        Task AddAccessAsync(string elementId, string userId);
        Task RemoveAccessAsync(string elementId, string userId);
        Task CreateHomeDirAsync(string userId);
        Task RemoveHomeDirAsync(string userId);
        Task SaveChangesAsync();
    }
}
