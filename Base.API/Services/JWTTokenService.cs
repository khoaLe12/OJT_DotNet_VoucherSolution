using Base.Core.Entity;
using Base.Core.Identity;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Base.API.Services
{
    public interface IJWTTokenService<T> where T : IdentityUser<Guid>
    {
        Task<string> CreateToken(T user);
    }

    public class JWTTokenService<T> : IJWTTokenService<T> where T : IdentityUser<Guid>
    {
        private readonly IConfiguration _configuration;
        private readonly IRoleService _roleService;

        public JWTTokenService(IConfiguration configuration, IRoleService roleService)
        {
            _configuration = configuration;
            _roleService = roleService;
        }

        public async Task<string> CreateToken(T user)
        {
            var roleClaims = await _roleService.GetRoleClaimsOfUser(user.Id);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.UserName),
            };

            if (roleClaims != null && user is User)
            {
                claims.AddRange(roleClaims);
            }

            if(user is Customer)
            {
                claims.Add(new Claim("scope", "Customer"));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
