using BackendGVK.Controllers;
using BackendGVK.Models;
using Neo4jClient;
using System.Security.Claims;

namespace BackendGVK.Services.CloudService
{
    public interface ICloud
    {
        GraphSet<FileModel> Files { get; set; }
        GraphSet<DirectoryModel> Directories { get; set; }
        Task<InternalElements> GetElementsAsync(string userId, CloudInputModel input);
        Task<bool> ChangeNameAsync(string userId, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(string userId, CloudInputModel input);
        Task DeleteAsync(string userId, CloudInputModel input);
        Task<InternalElements> GetRemovedElements(string userId);
        Task RestoreElementAsync(string userId, CloudInputModel input);
        Task<bool> AddFileAsync(string userId, FileModel file, CloudInputModel input);
        Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, CloudInputModel input);
        Task<string> GetPathAsync(string userId, string elementId, ElementTypes type);
        Task MoveToAsync(string userId, CloudInputModel input);
        Task CopyToAsync(string userId, CloudInputModel input);
        Task MoveToAccessModeAsync(string userId, CloudInputModel input);
        Task RemoveAccessAsync(CloudInputModel input, string forUserId = null!);
        Task CreateHomeDirAsync(string userId, string email);
        Task RemoveHomeDirAsync(string userId);
        Task<bool> isOwnerAsync(string userId, CloudInputModel inputModel);
        Task<bool> HasAccessAsync(string userId, CloudInputModel inputModel);
        Task<IEnumerable<InvitationModel>> GetInvitationsAsync(string userId);
        Task GrantAccessForAsync(ClaimsPrincipal principal, string toEmail, DirectoryModel directory);
        Task<bool> ExistsUserAsync(string email);
        Task AcceptInvitationAsync(string userId, InvitationModel invitation);
        Task<string> DeleteInvitationAsync(InvitationModel invitation);
        Task<string> GetHomeDirIdAsync(string userId);
        Task<string> GetDirSizeAsync(string userId, string dirId);
        Task SaveChangesAsync();
    }

    public class GraphSet<T>
    {
        private readonly ElementTypes _type;
        private readonly IGraphClient _client; 
        public GraphQuery<T> Query => new GraphQuery<T>(_type, _client);
        public GraphSet(ElementTypes type, IGraphClient client) {
            _type = type;
            _client = client;
        }   
    }
}
