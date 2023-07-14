using BackendGVK.Extensions;
using BackendGVK.Models;
using BackendGVK.Services.CloudService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BackendGVK.Controllers
{
    [Route("api/invitations")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InvitationsController : ControllerBase
    {
        private readonly ICloud _cloudManager;
        private readonly IAuthorizationService _authService;
        public InvitationsController(ICloud cloud, IAuthorizationService authorizationService)
        {
            _authService = authorizationService;
            _cloudManager = cloud;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvitations()
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            var invitations = await _cloudManager.GetInvitationsAsync(id);

            return Ok(invitations);
        }

        [HttpPost("send")]
        public async Task<IActionResult> GrantAccess(DirectoryModel model, [Required] string toEmail)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            var cloudInput = new CloudInputModel { TargetId = model.Id, TargetPath = model.CloudPath, Type = model.Type };

            bool isOwner = await _cloudManager.isOwnerAsync(id, cloudInput);
            if (!isOwner) return Forbid();

            await _cloudManager.GrantAccessForAsync(User, toEmail, model);

            return Ok();
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvitation(InvitationModel invitation)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            await _cloudManager.AcceptInvitationAsync(id, invitation);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> RejectInvitation(InvitationModel invitation)
        {
            string id = AuthHelper.GetUserId(User);
            if (id == null) return BadRequest();

            await _cloudManager.DeleteInvitationAsync(invitation);

            return Ok();
        }
    }
}
