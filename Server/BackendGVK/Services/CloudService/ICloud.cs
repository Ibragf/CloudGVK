using BackendGVK.Controllers;
using BackendGVK.Models;
using System.Security.Claims;

namespace BackendGVK.Services.CloudService
{
    public interface ICloud
    {
        Task<OutputElements> GetElementsAsync(string userId, string directory);
        Task<bool> ChangeNameAsync(string userId, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(string userId, string name, ElementTypes type);
        Task<bool> AddFileAsync(string userId, FileModel file, string destination);
        Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string destination);
        Task<string> GetPathAsync(string userId, string name, ElementTypes type);
        Task MoveToAsync(string userId, string name, string directoryDestination, ElementTypes type);
        Task CopyToAsync(string userId, string name, string destination, ElementTypes type);
        Task MoveToAccessModeAsync(string userId, string name, string destination, ElementTypes type);
        Task RemoveAccessAsync(string elementId, string userId);
        Task CreateHomeDirAsync(string userId, string email);
        Task RemoveHomeDirAsync(string userId);
        Task<bool> isOwnerAsync(string userId, string elementId, ElementTypes type);
        Task<bool> HasAccessAsync(string userId, string elementId, ElementTypes type);
        Task<IEnumerable<InvitationModel>> GetInvitationsAsync(string userId);
        Task GrantAccessForAsync(ClaimsPrincipal principal, string toEmail, DirectoryModel directory);
        Task<bool> ExistsUserAsync(string email);
        Task AcceptInvitationAsync(string userId, InvitationModel invitation);
        Task<string> DeleteInvitationAsync(InvitationModel invitation);
        Task<FileModel> GetFileByHashSumAsync(string hashSum);
        Task SaveChangesAsync();
    }
}
