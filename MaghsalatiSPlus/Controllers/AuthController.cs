
using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; 

namespace MaghsalatiSPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ShopOwner> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ShopOwner> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status409Conflict,
                    new { Message = "User with this email already exists." });
            }

            var user = new ShopOwner
            {
                UserName = dto.Email,
                Email = dto.Email,
                ShopName = dto.ShopName,
                PhoneNumber = dto.PhoneNumber,
                Location = dto.Location
            };

            if (dto.ProfileImageFile != null && dto.ProfileImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.ProfileImageFile.CopyToAsync(memoryStream);
                    user.ProfileImageData = memoryStream.ToArray();
                }
            }

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "User creation failed.", Errors = result.Errors });
            }

            return Ok(new { Message = "User account created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), 
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("shopName", user.ShopName)
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(30), 
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized(new { Message = "Invalid email or password." });
        }
    }
}