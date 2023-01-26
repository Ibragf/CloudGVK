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
        Task<InternalElements> GetElementsAsync(string userId, string directoryId);
        Task<bool> ChangeNameAsync(string userId, string oldName, string currentName, ElementTypes type);
        Task RemoveAsync(string userId, string elementId, ElementTypes type);
        Task<bool> AddFileAsync(string userId, FileModel file, string destinationId);
        Task<bool> AddDirectoryAsync(string userId, DirectoryModel dir, string destinationId);
        Task<string> GetPathAsync(string userId, string elementId, ElementTypes type);
        Task MoveToAsync(string userId, string elementId, string destinationId, ElementTypes type);
        Task CopyToAsync(string userId, string elementId, string destinationId, ElementTypes type);
        Task MoveToAccessModeAsync(string userId, string elementId, string destinationId, ElementTypes type);
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
        Task<string> GetHomeDirId(string userId);
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
