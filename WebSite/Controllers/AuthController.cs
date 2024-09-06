using DataModel;
using DataModel.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ScannerWebSite.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScannerWebSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ScannerModel _scannerModel;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, ScannerModel scannerModel, IConfiguration configuration)
        {
            _logger = logger;
            _scannerModel = scannerModel;
            _configuration = configuration;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest authReq)
        {
            // Side-channel attack protection
            System.Threading.Thread.Sleep(new Random().Next(50, 200));


           /*
            {
                RNGCryptoServiceProvider cryptographicServiceProvider = new RNGCryptoServiceProvider();
                byte[] salt = new byte[24];
                cryptographicServiceProvider.GetBytes(salt);
                var hash = new Rfc2898DeriveBytes(authReq.Password, salt, 1000).GetBytes(24);
                _scannerModel.AddItem(new User() { Username = authReq.Username, Password = Convert.ToBase64String(hash), Salt = Convert.ToBase64String(salt), Id = Guid.NewGuid() });
            }
        */

            var user = _scannerModel.GetUser(authReq.Username);
            if (user != null)
            {
                var hash = new Rfc2898DeriveBytes(authReq.Password, Convert.FromBase64String(user.Salt), 1000).GetBytes(24);
                if (user.Password == Convert.ToBase64String(hash))
                {
                    return Ok(new AuthenticateResponse() { Username = user.Username, Token = generateJwtToken(user) });
                }
            }
            return Unauthorized();
        }

        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["AppSettings:JwtTokenKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
