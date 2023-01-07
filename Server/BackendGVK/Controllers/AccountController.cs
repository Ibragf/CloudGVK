using BackendGVK.Models;
using BackendGVK.Services.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackendGVK.Controllers
{
    [Route("api")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<JwtSettings> jwtSettings) {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("register")]
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
                await _userManager.AddClaimAsync(user, new Claim("Email", user.Email));


                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("ProblemDetails", error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> SignIn(SignInModel model)
        {
            string token;
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user==null)
            {
                ModelState.AddModelError("ProblemDetails", "Could not find user.");
                return NotFound(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("ProblemDetails", "Too many login attempts. Login to the account was blocked for 10 minutes.");
                return BadRequest(ModelState);
            }
            if (result.Succeeded)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                token = GetToken(claims);
            }
            else
            {
                ModelState.AddModelError("ProblemDetails", "Invalid password or login");
                return BadRequest(ModelState);
            }

            return Ok(token);
        }

        private string GetToken(IEnumerable<Claim> claims)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                signingCredentials: new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256),
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(5)
            );

            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }
    }

    public class SignInModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Minimum length is 10")]
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [MaxLength(30, ErrorMessage ="Maximum length is 30")]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        public string UserName { get; set;}
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
