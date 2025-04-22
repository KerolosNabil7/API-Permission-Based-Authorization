using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ApiPermissionBasedAuthorization.Helpers;
using ApiPermissionBasedAuthorization.Models;

namespace ApiPermissionBasedAuthorization.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly JWT _jwt;
        public AuthService(UserManager<IdentityUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            //check if the given email exists in the DB or not
            //if exists it can not be register again
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel
                {
                    Message = "Email is already registered!",
                    IsAuthenticated = false,
                };

            //check if the given username exists in the DB or not
            //if exists it can not be register again
            if (await _userManager.FindByNameAsync(model.Username) != null)
                return new AuthModel
                {
                    Message = "Username is already registered!",
                    IsAuthenticated = false,
                };

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password); //Pass the Password to encrypt it during user creation or DB saving

            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description}, ";

                return new AuthModel
                {
                    Message = errors,
                    IsAuthenticated = false,
                };
            }

            //To add the created user to Role User
            await _userManager.AddToRoleAsync(user, "Basic");

            var jwtToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Email = user.Email,
                Username = user.UserName,
                IsAuthenticated = true,
                Message = "The user created Successfully",
                ExpiresOn = jwtToken.ValidTo,
                Roles = new List<string> { "Basic" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            };
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            //In  case of there is no user with the provided Email or the password incorrect
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect";
                return authModel;
            }

            var jwtToken = await CreateJwtToken(user);
            var RolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtToken.ValidTo;
            authModel.Roles = RolesList.ToList();

            return authModel;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(IdentityUser user)
        {
            //To create a jwt token

            //1. u should obtain the user claims
            var UserClaims = await _userManager.GetClaimsAsync(user);

            //2. prefer to obtain the user roles to return them with the generated token for front-end developer
            var UserRoles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            foreach (var role in UserRoles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(UserClaims)
            .Union(roleClaims);

            //generate symmetricSecuirtyKey
            var symmetricSecuirtyKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var signingCredentials = new SigningCredentials(symmetricSecuirtyKey, SecurityAlgorithms.HmacSha256);

            //what values that will be used during JWT token generation
            var jwtToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials
                );

            return jwtToken;
        }
    }
}
