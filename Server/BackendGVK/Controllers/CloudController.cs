using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("change/{name}")]
        public async Task<IActionResult> UpdateName([FromRoute] string name)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.ChangeNameAsync(id, oldName, currentName, )
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
