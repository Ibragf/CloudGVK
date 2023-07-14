using BackendGVK.Extensions;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Controllers
{
    [Route("api/elements")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ElementsController : ControllerBase
    {
        private readonly ICloud _cloudManager;
        public ElementsController(ICloud cloud)
        {
            _cloudManager = cloud;
        }

        [HttpGet]
        public async Task<IActionResult> GetElements(CloudInputModel input)
        {
            string id = AuthHelper.GetUserId(User);
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

        [HttpDelete("access")]
        public async Task<IActionResult> RemoveAccess(string? forUserId, CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model);
            if (!isOwner) return Forbid();

            await _cloudManager.RemoveAccessAsync(model, forUserId!);

            return Ok();
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopyElement(CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null || model.DestinationId == null) return BadRequest();

            bool isAllowed = await AuthHelper.isAllowedAsync(id, model, _cloudManager);
            if (isAllowed)
            {
                await _cloudManager.CopyToAsync(id, model);
            }
            else return Forbid();

            return Ok();
        }

        [HttpPut("move")]
        public async Task<IActionResult> MoveElement(CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null || model.DestinationId == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model);

            if (isOwner)
            {
                await _cloudManager.MoveToAsync(id, model);
                return Ok();
            }

            bool hasAccess = await _cloudManager.HasAccessAsync(id, model);

            if (hasAccess)
            {
                await _cloudManager.MoveToAccessModeAsync(id, model);
                return Ok();
            }

            return Forbid();
        }

        [HttpPut("name")]
        public async Task<IActionResult> UpdateName(CloudInputModel model, [Required][StringLength(50, MinimumLength = 1)] string currentName)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            bool isAllowed = await AuthHelper.isAllowedAsync(id, model, _cloudManager);

            if (isAllowed)
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

        [HttpPost("dirs")]
        public async Task<IActionResult> AddDirectory([Required][StringLength(50, MinimumLength = 1)] string dirName, CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
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

        [HttpGet("dirs/size")]
        public async Task<IActionResult> GetSize(string dirId)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            string dirSize = await _cloudManager.GetDirSizeAsync(id, dirId);

            return Ok(dirSize);
        }
    }
}
