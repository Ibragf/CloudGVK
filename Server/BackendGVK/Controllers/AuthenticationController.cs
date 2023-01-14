using BackendGVK.Db;
using BackendGVK.Models;
using BackendGVK.Services;
using BackendGVK.Services.Configs;
using BackendGVK.Services.EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenManager tokenManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenManager = tokenManager;
            _emailsender = emailSender;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            ApplicationUser user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Name, model.UserName)
                };
                await _userManager.AddClaimsAsync(user, claims);


                await _signInManager.SignInAsync(user, isPersistent: false);
                await _emailsender.SendEmailAsync(model.Email, "Confirm Email", code);
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
        public async Task<IActionResult> SignIn(SignInModel model)
        {
            string token;
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(!user.EmailConfirmed)
            {
                ModelState.AddModelError("errors", "Email not confirmed.");
                return BadRequest(ModelState);
            }
            if (user == null)
            {
                ModelState.AddModelError("errors", "Could not find user.");
                return NotFound(ModelState);
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
                token = await _tokenManager.GenerateTokenAsync(user.Id, claims, model.FingerPrint);
                if (token == null) return BadRequest();
            }
            else
            {
                ModelState.AddModelError("errors", "Invalid password or login");
                return BadRequest(ModelState);
            }

            return Ok(token);
        }

        [HttpPost("logout")]
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
        public async Task<IActionResult> ConfirmEmail(string code, string userEmail)
        {
            bool isSuccess = true;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded) return Ok();
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
            public string Email { get; set; }
            [Required]
            [MinLength(10, ErrorMessage = "Minimum length is 10")]
            public string Password { get; set; }
            [Required]
            public string FingerPrint { get; set; }
        }

        public class RegisterModel
        {
            [Required]
            [MaxLength(30, ErrorMessage = "Maximum length is 30")]
            [MinLength(3, ErrorMessage = "Minimum length is 3")]
            public string UserName { get; set; }
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public class RefreshModel
        {
            [Required]
            public string Token { get; set; }
            [Required]
            public string FingerPrint { get; set; }
        }

    }

}
