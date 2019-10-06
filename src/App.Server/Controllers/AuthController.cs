using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Server.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Server.Controllers
{
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthOptions _options;
        
        public AuthController(IOptionsSnapshot<AuthOptions> options)
        {
            _options = options.Value;
        }

        [HttpGet("token")]
        public string Issue([FromQuery] string nick)
        {
            var securityKey = Encoding.UTF8.GetBytes(_options.SecurityKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.Name, nick), 
                }),
                Expires = DateTime.UtcNow.AddMinutes(_options.ExpiresIn),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey),  SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}