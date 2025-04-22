using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPermissionBasedAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet("GetUsersAsync")]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }


    }
}
