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

        [HttpGet("elements")]
        public async Task<IActionResult> GetElements(CloudInputModel input)
        {
            string id = GetUserId();
            string homeDirId = User.Claims.FirstOrDefault(x => x.Type == "HomeDir")?.Value!;
            if (id == null || homeDirId == null) return BadRequest();

            InternalElements elements;
            if (input == null)
            {
                input = new CloudInputModel { TargetId = homeDirId, Type = ElementTypes.Directory };
                elements = await _cloudManager.GetElementsAsync(id, input);
            }
            else
                elements = await _cloudManager.GetElementsAsync(id, input);

            return Ok(elements);
        }

        [HttpGet("dirs/size")]
        public async Task<IActionResult> GetSize(string dirId)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            string dirSize = await _cloudManager.GetDirSizeAsync(id, dirId);

            return Ok(dirSize);
        }

        [HttpGet("invitations")]
        public async Task<IActionResult> GetInvitations()
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            var invitations = await _cloudManager.GetInvitationsAsync(id);

            return Ok(invitations);
        }

        [HttpPost("invintations/accept")]
        public async Task<IActionResult> AcceptInvitation(InvitationModel invitation)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.AcceptInvitationAsync(id, invitation);

            return Ok();
        }

        [HttpPost("invintations/reject")]
        public async Task<IActionResult> RejectInvitation(InvitationModel invitation)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.DeleteInvitationAsync(invitation);

            return Ok();
        }

        [HttpPost("invintations/send")]
        public async Task<IActionResult> GrantAccess(DirectoryModel model,[Required] string toEmail)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            var cloudInput = new CloudInputModel { TargetId = model.Id, TargetPath = model.CloudPath, Type = model.Type };

            bool isOwner = await _cloudManager.isOwnerAsync(id, cloudInput);
            if (!isOwner) return Forbid();

            await _cloudManager.GrantAccessForAsync(User, toEmail, model);

            return Ok();
        }

        [HttpPut("elements/access/remove")]
        public async Task<IActionResult> RemoveAccess(string? forUserId, CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model);
            if (!isOwner) return Forbid();

            await _cloudManager.RemoveAccessAsync(model, forUserId!);

            return Ok();
        }

        [HttpPost("elements/copy")]
        public async Task<IActionResult> CopyElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null || model.DestinationId==null) return BadRequest();

            bool isAllowed = await isAllowedAsync(id, model);
            if (isAllowed)
            {
                await _cloudManager.CopyToAsync(id, model);
            }
            else return Forbid();

            return Ok();
        }

        [HttpPut("elements/move")]
        public async Task<IActionResult> MoveElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null || model.DestinationId == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model);

            if (isOwner)
            {
                await _cloudManager.MoveToAsync(id, model);
                return Ok();
            }

            bool hasAccess = await _cloudManager.HasAccessAsync(id, model);

            if(hasAccess)
            {
                await _cloudManager.MoveToAccessModeAsync(id, model);
                return Ok();
            }

            return Forbid();
        }

        [HttpPut("elements/name/change")]
        public async Task<IActionResult> UpdateName(CloudInputModel model, [Required][StringLength(50, MinimumLength = 1)] string currentName)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool isAllowed = await isAllowedAsync(id, model);

            if(isAllowed)
            {
                if (model.Type == ElementTypes.File)
                    await _cloudManager.Files.Query
                        .Where(nameof(FileModel.Id), model.TargetId)
                        .Update(nameof(FileModel.UntrustedName), currentName)
                        .ExecuteAsync();
                if (model.Type == ElementTypes.Directory)
                    await _cloudManager.Directories.Query
                        .Where(nameof(DirectoryModel.Id), model.TargetId)
                        .Update(nameof(DirectoryModel.UntrustedName), currentName)
                        .ExecuteAsync();

                return Ok();
            }
            else 
                return Forbid();
        }

        [HttpPost("dirs/add")]
        public async Task<IActionResult> AddDirectory([Required][StringLength(50, MinimumLength = 1)] string dirName, CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            DirectoryModel dir = new DirectoryModel
            {
                Id = Guid.NewGuid().ToString(),
                UntrustedName = dirName,
                CloudPath = Path.Combine(model.DestinationPath, dirName),
                OwnerId = id,
                isAdded = true
            };

            bool result = await _cloudManager.AddDirectoryAsync(id, dir, model);

            if (result)
            {
                return Ok(dir);
            }
            else
            {
                ModelState.AddModelError("errors", "Directory already exists.");
                return BadRequest(ModelState);
            }
        }

        private string GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == "Id");
            if (claim == null) return null!;
            return claim.Value;
        }

        private async Task<bool> isAllowedAsync(string userId, CloudInputModel model)
        {
            bool hasAccess = await _cloudManager.HasAccessAsync(userId, model);
            bool isOwner = await _cloudManager.isOwnerAsync(userId, model);

            return hasAccess || isOwner;
        }
    }

    public class InternalElements
    {
        public IEnumerable<FileModel> Files { get; set; }
        public IEnumerable<DirectoryModel> Directories { get; set; }
        public IEnumerable<DirectoryModel> Shared { get; set; }
    }

    public class CloudInputModel
    {
        public string TargetId { get; set; } = null!;
        public string DestinationId { get; set; } = null!;
        public string TargetPath { get; set; } = null!;
        public string DestinationPath { get; set; } = null!;
        [Required]
        public ElementTypes Type { get; set; }
    }
}
