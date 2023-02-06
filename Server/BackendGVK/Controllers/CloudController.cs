using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Controllers
{
    [Route("api/cloud")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CloudController : ControllerBase
    {
        private readonly ICloud _cloudManager;
        private readonly IAuthorizationService _authService;
        public CloudController(ICloud cloud, IAuthorizationService authorizationService) 
        {
            _authService = authorizationService;
            _cloudManager = cloud;
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvitation(InvitationModel invitation)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.AcceptInvitationAsync(id, invitation);

            return Ok();
        }

        [HttpPost("grant")]
        public async Task<IActionResult> GrantAccess(DirectoryModel model,[Required] string toEmail)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model.Id, model.Type);
            if (!isOwner) return Forbid();

            await _cloudManager.GrantAccessForAsync(User, toEmail, model);

            return Ok();
        }

        [HttpPost("copyto")]
        public async Task<IActionResult> CopyElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null || model.DestinationId==null) return BadRequest();

            var authResult = await _authService.AuthorizeAsync(User, model, "isAllowed");
            if (authResult.Succeeded)
            {
                await _cloudManager.CopyToAsync(id, model.ElementId, model.DestinationId, model.Type);
            }
            else return Forbid();

            return Ok();
        }

        [HttpPost("moveto")]
        public async Task<IActionResult> MoveElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null || model.DestinationId == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model.ElementId, model.Type);

            if (isOwner)
            {
                await _cloudManager.MoveToAsync(id, model.ElementId, model.DestinationId, model.Type);
                return Ok();
            }

            bool hasAccess = await _cloudManager.HasAccessAsync(id, model.ElementId, model.Type);

            if(hasAccess)
            {
                await _cloudManager.MoveToAccessModeAsync(id, model.ElementId, model.DestinationId, model.Type);
                return Ok();
            }

            return Forbid();
        }

        [HttpGet("add/dir")]
        public async Task<IActionResult> AddDirectory([Required][StringLength(50, MinimumLength = 1)] string dirName,[Required] string destinationId)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            string destPath = await _cloudManager.GetPathAsync(id, destinationId, ElementTypes.Directory);
            if (destPath == null)
            {
                ModelState.AddModelError("errors", "Destination path doesn't exists.");
                return BadRequest(ModelState);
            }

            DirectoryModel dir = new DirectoryModel
            {
                Id = Guid.NewGuid().ToString(),
                UntrustedName = dirName,
                CloudPath = Path.Combine(destPath, dirName),
                Size = "0",
                isAdded = true
            };

            bool result = await _cloudManager.AddDirectoryAsync(id, dir, destinationId);
            
            if(result)
            {
                dir.isAdded = true;
                return Ok(dir);
            }
            else
            {
                ModelState.AddModelError("errors", "Directory already exists.");
                return BadRequest(ModelState);
            }
        }

        [HttpPost("change")]
        public async Task<IActionResult> UpdateName(CloudInputModel model, [Required][StringLength(50, MinimumLength = 1)] string currentName)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();
            bool hasAccess = await _cloudManager.HasAccessAsync(id, model.ElementId, model.Type);
            bool isOwner = await _cloudManager.isOwnerAsync(id, model.ElementId, model.Type);
            
            if(hasAccess || isOwner )
            {
                if (model.Type == ElementTypes.File)
                    await _cloudManager.Files.Query
                        .Where(nameof(FileModel.Id), model.ElementId)
                        .Update(nameof(FileModel.UntrustedName), currentName)
                        .ExecuteAsync();
                if (model.Type == ElementTypes.Directory)
                    await _cloudManager.Directories.Query
                        .Where(nameof(DirectoryModel.Id), model.ElementId)
                        .Update(nameof(DirectoryModel.UntrustedName), currentName)
                        .ExecuteAsync();

                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("elements")]
        public async Task<IActionResult> GetElements(string directoryId)
        {
            string id = GetUserId();
            string homeDirId = User.Claims.FirstOrDefault(x => x.Type == "HomeDir")?.Value!;
            if(id == null || homeDirId==null) return BadRequest();

            InternalElements elements;
            if (directoryId == null)
                elements = await _cloudManager.GetElementsAsync(id, homeDirId);
            else if (directoryId != null)
                elements = await _cloudManager.GetElementsAsync(id, directoryId);
            else
                return BadRequest();

            return Ok(elements);
        }

        private string GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == "Id");
            if (claim == null) return null!;
            return claim.Value;
        }
    }

    public class InternalElements
    {
        public IEnumerable<Element> Files { get; set; }
        public IEnumerable<DirectoryModel> Directories { get; set; }
        public IEnumerable<DirectoryModel> Shared { get; set; }
    }

    public class CloudInputModel
    {
        [Required]
        public string ElementId { get; set; } = null!;
        public string DestinationId { get; set; } = null!;
        [Required]
        public ElementTypes Type { get; set; }
    }
}
