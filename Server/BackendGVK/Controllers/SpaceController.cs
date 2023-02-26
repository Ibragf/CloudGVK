using BackendGVK.Extensions;
using BackendGVK.Services.SpaceService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Controllers
{
    [Route("api/space")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SpaceController : ControllerBase
    {
        private readonly SpaceManager _spaceManager;
        public SpaceController(SpaceManager spaceManager) {
            _spaceManager = spaceManager;
        }

        [HttpPost("upload")]
        [DisableFormValueModelBinding]
        [RequestFormLimits(MultipartBodyLengthLimit = 1024L*1024L*1024L*10L)]
        [RequestSizeLimit(1024L * 1024L * 1024L * 10L)]
        public async Task<IActionResult> UploadLargeFiles([Required] string directoryId, [Required] string connectionId)
        {
            var files = await _spaceManager.UploadFiles(directoryId, connectionId);

            return Ok(files);
        }
    }
}
