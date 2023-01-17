using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BackendGVK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CloudController : ControllerBase
    {
        public CloudController() 
        {

        }

        /*[HttpGet("{directory}")]
        public async Task<IActionResult> GetElements()
        {
        
        }*/
    }
}
