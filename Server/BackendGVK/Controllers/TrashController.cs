using BackendGVK.Extensions;
using BackendGVK.Models;
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
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            var internalElements = await _cloudManager.GetRemovedElements(id);

            return Ok(internalElements);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveElement(CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            await _cloudManager.RemoveAsync(id, model);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteElement(CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            await _cloudManager.DeleteAsync(id, model);

            return Ok();
        }

        [HttpPut("restore")]
        public async Task<IActionResult> RestoreElement(CloudInputModel model)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            await _cloudManager.RestoreElementAsync(id, model);

            return Ok();
        }
    }
}
