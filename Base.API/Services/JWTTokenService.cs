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
    public interface IJWTTokenService
    {
        Task<string> CreateToken(User user);
        string CreateToken(Customer customer);
    }

    public class JWTTokenService : IJWTTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public JWTTokenService(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<string> CreateToken(User user)
        {
            var rolesList = await _userService.GetRolesByUserId(user.Id);
            string roles = "";
            if(rolesList != null)
            {
                foreach (Role r in rolesList)
                {
                    roles = roles + " " + r.Name;
                }
            }

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.UserName),
                new System.Security.Claims.Claim("scope", roles.Trim()),
            };

            /*var roleClaims = new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim("scope", roles.Trim())
            };

            foreach(var claim in roleClaims)
            {
                claims.Add(claim);
            }*/

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateToken(Customer customer)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, customer.UserName),
                new System.Security.Claims.Claim("scope", "Customer")
            };

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
