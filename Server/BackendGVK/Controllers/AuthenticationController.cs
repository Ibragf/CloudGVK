using BackendGVK.Models;
using BackendGVK.Services;
using BackendGVK.Services.CloudService;
using BackendGVK.Services.EmailSender;
using BackendGVK.Services.TokenManagerService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BackendGVK.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenManager _tokenManager;
        private readonly IEmailSender _emailsender;
        private readonly GoogleCaptcha _googleCaptcha;
        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenManager tokenManager,
            IEmailSender emailSender, GoogleCaptcha googleCaptcha)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenManager = tokenManager;
            _emailsender = emailSender;
            _googleCaptcha = googleCaptcha;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Name, model.UserName)
                };
                await _userManager.AddClaimsAsync(user, claims);

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string html = _emailsender.GetHtmlForConfirmationToken(code);
                await _emailsender.SendEmailAsync(model.Email, "Confirm Email", html);

                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("errors", error.Description);
                }
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> SignIn(SignInModel model, [FromServices] ICloud cloud)
        {
            string token;
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("errors", "Could not find user.");
                return NotFound(ModelState);
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("errors", "Email not confirmed.");
                return BadRequest(ModelState);
            }
            if(user.AccessFailedCount > 5) {
                bool isFailed;
                if(model.RecaptchaResponse==null)
                {
                    isFailed = true;
                    ModelState.AddModelError("errors", "RecaptchaResponse is required.");
                }
                else
                {
                    var captchaResult = await _googleCaptcha.VerifyTokenAsync(model.RecaptchaResponse);
                    isFailed = !captchaResult;
                    if (isFailed) ModelState.AddModelError("errors", "reCaptcha token verification failed.");
                }
                if (isFailed) return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("errors", "Too many login attempts. Login to the account was blocked for 10 minutes.");
                return BadRequest(ModelState);
            }
            if (result.Succeeded)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var homeDirId = await cloud.GetHomeDirId(user.Id);
                if (homeDirId == null) return BadRequest();

                claims.Add(new Claim("Id", user.Id));
                claims.Add(new Claim("HomeDir", homeDirId));

                token = await _tokenManager.GenerateTokenAsync(user.Id, claims, model.FingerPrint);
                if (token == null)
                {
                    ModelState.AddModelError("errors", $"Token is null.{model.Password} - {model.FingerPrint}-{user.Id}");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError("errors", "Invalid password or login");
                return BadRequest(ModelState);
            }

            var invitations = await cloud.GetInvitationsAsync(user.Id);
            var output = new
            {
                token = token,
                invitations = invitations,
            };

            return Ok(output);
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> LogOut(string token)
        {
            bool result = await _tokenManager.RemoveTokenAsync(token);
            if (result) return Ok();
            else
            {
                ModelState.AddModelError("errors", "Token is invalid probably.");
                return BadRequest(ModelState);
            }

        }

        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken(RefreshModel model)
        {
            string refresh=await _tokenManager.RefreshTokenAsync(model.Token, model.FingerPrint);
            if(refresh == null)
            {
                ModelState.AddModelError("errors", "Token or fingerPrint is invalid.");
                return BadRequest(ModelState);
            }
            else return Ok(refresh);
        }

        [HttpGet("confirm/email")]
        public async Task<IActionResult> ConfirmEmail(string code, [EmailAddress] string userEmail, [FromServices] ICloud cloud)
        {
            bool isSuccess = true;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    await cloud.CreateHomeDirAsync(user.Id, userEmail);
                    return Ok();
                }
                else
                {
                    isSuccess = false;
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("errors", error.Description);
                    }
                }
            }
            else
            {
                isSuccess = false;
                ModelState.AddModelError("errors", $"User with email {userEmail} does not exist.");
            }

            return isSuccess ? Ok() : BadRequest(ModelState);
        }

        public class SignInModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = null!;
            [Required]
            [MinLength(10, ErrorMessage = "Minimum length is 10")]
            public string Password { get; set; } = null!;
            [Required]
            public string FingerPrint { get; set; } = null!;
            public string? RecaptchaResponse { get; set; }
        }

        public class RegisterModel
        {
            [Required]
            [MaxLength(30, ErrorMessage = "Maximum length is 30")]
            [MinLength(3, ErrorMessage = "Minimum length is 3")]
            public string UserName { get; set; } = null!;
            [Required]
            [EmailAddress]
            public string Email { get; set; } = null!;
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = null!;
        }

        public class RefreshModel
        {
            [Required]
            public string Token { get; set; } = null!;
            [Required]
            public string FingerPrint { get; set; } = null!;
        }

    }

}
