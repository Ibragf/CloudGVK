using BackendGVK.Db;
using BackendGVK.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendGVK.Services.CloudService
{
    public class CloudManager : ICloud
    {
        private readonly AppDbContext _dbContext;
        public CloudManager(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }   
        public async Task AddAsync(Element element, string directory)
        {
            if(element == null || directory == null) throw new ArgumentNullException();

            switch (element.Type)
            {
                case ElementTypes.File:
                    await _dbContext.Files.AddAsync((FileModel) element);
                    break;
                case ElementTypes.Directory:
                    await _dbContext.Directories.AddAsync((DirectoryModel) element);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public async Task ChangeNameAsync(ApplicationUser user, string oldName, string currentName, ElementTypes type)
        {
            if(user== null || oldName==null || currentName ==null) throw new ArgumentNullException();

            switch (type)
            {
                case ElementTypes.File:
                    FileModel? file = await _dbContext.Files.FirstOrDefaultAsync(x => x.OwnerId == user.Id && x.UntrustedName == oldName);
                    if (file == null) return;
                    file.UntrustedName = currentName;
                    break;
                case ElementTypes.Directory:
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public async Task<IEnumerable<Element>> GetElementsAsync(ApplicationUser user, string directory)
        {
            List<Element> elements = null!;
            var folder = await _dbContext.Directories.Where(x => x.OwnerId == user.Id && x.Name == directory)
                .Include(x => x.Directories).Include(x => x.Files).FirstOrDefaultAsync();
            
            if(folder == null) return elements;

            elements = new List<Element>();
            if (folder.Directories != null) elements.AddRange(folder.Directories);
            if (folder.Files != null) elements.AddRange(folder.Files);
            if (directory == "home")
            {
                var userWithAccess = await _dbContext.Users.Where(x => x.Id == user.Id).Include(x => x.SharedDirectories).Include(x => x.SharedFiles).FirstOrDefaultAsync()!;
                if(userWithAccess.SharedDirectories!=null) elements.AddRange(userWithAccess.SharedDirectories);
                if(userWithAccess.SharedFiles!=null) elements.AddRange(userWithAccess.SharedFiles);
            }

            return elements;
        }

        public async Task<string> GetPathAsync(ApplicationUser user, string name, ElementTypes type)
        {
            Element? element;
            switch (type)
            {
                case ElementTypes.File:
                    element = await _dbContext.Files.FirstOrDefaultAsync(x => x.UntrustedName == name && x.OwnerId == user.Id);
                    break;
                case ElementTypes.Directory:
                    element = await _dbContext.Directories.FirstOrDefaultAsync(x => x.Name == name && x.OwnerId == user.Id);
                    break;
                default:
                    throw new ArgumentException();
            }

            if (element == null) return null;
            
            return element.Path;
        }

        public Task MoveToAsync(ApplicationUser user, string name, string directoryDestination, ElementTypes type, bool isCopy = false)
        {
            
        }

        public Task RemoveAsync(ApplicationUser user, string name, ElementTypes type)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
