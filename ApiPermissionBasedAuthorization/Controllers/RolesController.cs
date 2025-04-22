using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPermissionBasedAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet("GetRolesAsync")]
        public async Task<IActionResult> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost("AddRoleAsync")]
        public async Task<IActionResult> AddRoleAsync([FromForm]string roleName)
        {
            // Validate the role name
            if (string.IsNullOrEmpty(roleName))
                return BadRequest("Role name cannot be empty.");

            // Check if the role already exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
                return BadRequest($"Role {roleName} already exists.");

            // Create a new role
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return Ok($"Role {roleName} created successfully.");
            // If there are errors, return them
            return BadRequest($"Failed to create role {roleName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
