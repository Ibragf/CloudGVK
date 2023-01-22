using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace BackendGVK.Controllers
{
    [Route("api/cloud")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CloudController : ControllerBase
    {
        private readonly ICloud _cloudManager;
        public CloudController(ICloud cloud) 
        {
            _cloudManager = cloud;
        }
        [HttpGet("accept")]
        
        public async Task<IActionResult> AcceptInvitation(InvitationModel invitation)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.AcceptInvitationAsync(id, invitation);

            return Ok();
        }

        [HttpGet("grant")]
        public async Task<IActionResult> GrantAccess(DirectoryModel model,[Required] string toEmail)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, model.Id, model.Type);
            if (!isOwner) return Forbid();

            await _cloudManager.GrantAccessForAsync(User, toEmail, model);

            return Ok();
        }

        [HttpGet("copyto/{destination}")]
        public async Task<IActionResult> CopyElement(Element element,[Required] string destination)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.CopyToAsync(id, element.UntrustedName, destination, element.Type);

            return Ok();
        }

        [HttpGet("moveto/{destination}")]
        public async Task<IActionResult> MoveElement(Element element,[Required] string destination)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool isOwner = await _cloudManager.isOwnerAsync(id, element.Id, element.Type);

            if (isOwner)
            {
                await _cloudManager.MoveToAsync(id, element.UntrustedName, destination, element.Type);
                return Ok();
            }

            bool hasAccess = await _cloudManager.HasAccessAsync(id, element.Id, element.Type);

            if(hasAccess)
            {
                await _cloudManager.MoveToAccessModeAsync(id, element.UntrustedName, destination, element.Type);
                return Ok();
            }

            return Forbid();
        }

        [HttpGet("add/dir")]
        public async Task<IActionResult> AddDirectory([Required][StringLength(50, MinimumLength = 1)] string dirName,[Required] string destination)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            string destPath = await _cloudManager.GetPathAsync(id, destination, ElementTypes.Directory);

            DirectoryModel dir = new DirectoryModel
            {
                Id = Guid.NewGuid().ToString(),
                UntrustedName = dirName,
                Path = Path.Combine(destPath, dirName),
                Size = 0
            };

            bool result = await _cloudManager.AddDirectoryAsync(id, dir, destination);
            
            if(result) return Ok(dir);
            return BadRequest();
        }

        [HttpGet("change")]
        public async Task<IActionResult> UpdateName([Required][StringLength(50, MinimumLength = 1)] string currentName, Element element)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            bool result = await _cloudManager.ChangeNameAsync(id, element.UntrustedName, currentName, element.Type);

            if(result) return Ok();
            return BadRequest();
        }

        [HttpGet("{directory}")]
        public async Task<IActionResult> GetElements([FromRoute] string directory)
        {
            string id = GetUserId();
            if(id == null) return BadRequest();

            var elements = await _cloudManager.GetElementsAsync(id, directory);

            return Ok(elements);
        }

        private string GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == "Id");
            if (claim == null) return null!;

            return claim.Value;
        }
    }

    public class OutputElements
    {
        public IEnumerable<FileModel> Files { get; set; }
        public IEnumerable<DirectoryModel> Directories { get; set; }
        public IEnumerable<DirectoryModel> Shared { get; set; }
    }
}
