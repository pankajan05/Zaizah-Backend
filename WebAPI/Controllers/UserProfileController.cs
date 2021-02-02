using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Model;

namespace WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IMongoDbData _mongoDbData;
        private string username;

        public UserProfileController(ILogger<AuthController> logger, IMongoDbData mongoDbData, IHttpContextAccessor httpContextAccessor)
        {
            username = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            _logger = logger;
            _mongoDbData = mongoDbData;
        }

        [HttpGet]
        [Authorize]
        public async Task<Object> GetUserProfile()
        {
            var user = await _mongoDbData.GetUserDetails(username);
            return user;

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserDetails(UserDetails user)
        {
             await _mongoDbData.Update(username, user );
            return Ok(new { message = "successfully registered." });
        }
    }
}
