using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendGVK.Controllers
{
    [Route("api/trash/elements")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TrashController : ControllerBase
    {
        private readonly ICloud _cloudManager;
        public TrashController(ICloud cloud)
        {
            _cloudManager = cloud;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync() 
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            var internalElements = await _cloudManager.GetRemovedElements(id);

            return Ok(internalElements);
        }

        [HttpPost("add")]
        public async Task<IActionResult> RemoveElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.RemoveAsync(id, model);

            return Ok();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.DeleteAsync(id, model);

            return Ok();
        }

        [HttpPut("restore")]
        public async Task<IActionResult> RestoreElement(CloudInputModel model)
        {
            string id = GetUserId();
            if (id == null) return BadRequest();

            await _cloudManager.RestoreElementAsync(id, model);

            return Ok();
        }

        private string GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(x => x.Type == "Id");
            if (claim == null) return null!;
            return claim.Value;
        }
    }
}
