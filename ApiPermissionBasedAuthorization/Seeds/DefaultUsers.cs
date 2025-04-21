using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ApiPermissionBasedAuthorization.Constants;

namespace ApiPermissionBasedAuthorization.Seeds
{
    public static class DefaultUsers
    {
        //Seed the DB (Users table) with 1 Basic user
        public static async Task SeedBasicUserAsync(UserManager<IdentityUser> userManager)
        {
            var defaultUser = new IdentityUser
            {
                UserName = "basicuser@domain.com",
                Email = "basicuser@domain.com",
                EmailConfirmed = true,
            };

            // Check if the user doesn't exist in BD => Add it 
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if(user is null)
            {
                await userManager.CreateAsync(defaultUser, "Kerolos123#");
                await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
            }
        }

        //Seed the DB (Users table) with 1 SuperAdmin user
        public static async Task SeedSuperAdminUserAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed the DB (Users table) with 1 Basic user
            var defaultUser = new IdentityUser
            {
                UserName = "superadmin@domain.com",
                Email = "superadmin@domain.com",
                EmailConfirmed = true,
            };

            // Check if the user doesn't exist in BD => Add it 
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user is null)
            {
                await userManager.CreateAsync(defaultUser, "Kerolos123#");
                //add the user to all roles (becasue the super admin has all the permissions)
                await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.SuperAdmin.ToString());
            }

            //TODO: Seed Claims
            await roleManager.SeedClaimsForSuperUser();
        }

        //Use is as a reflection/Extension method (this) => so i can use the method using the roleManager
        private static async Task SeedClaimsForSuperUser(this RoleManager<IdentityRole> roleManager)
        {
            var SuperAdminRole = await roleManager.FindByNameAsync(Roles.SuperAdmin.ToString());

            //After i get the role => I want to add Permission/Claims on it
            await roleManager.AddPermissionClaims(SuperAdminRole, "Products");
        }

        //Extension method
        //Takes 2 parameters (Role to assign the permission to it, ModuleName to get the 4 permissions of the module and add them to the role)
        public static async Task AddPermissionClaims(this RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
        {
            //Check if the role already has claims or not
            var allClaims = await roleManager.GetClaimsAsync(role);

            //I want to generate the permissions names like (Permissions.ModuleName.PermissionName=Permissions.Products.Create)
            List<string> allPermissions = Permissions.GeneratePermissionsList(module);

            foreach (var permission in allPermissions)
            {
                //Check the claims list specially the claims of type permission
                //if there is no permission with value == permission => add the permssion to the claims list
                if(!allClaims.Any(c=>c.Type=="Permission" && c.Value==permission))
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }
}
