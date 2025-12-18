using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SupportDeskBE.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SupportDeskBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly IConfiguration _cfg;

        public AuthController(UserManager<IdentityUser> users, IConfiguration cfg)
        {
            _users = users;
            _cfg = cfg;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _users.AddToRoleAsync(user, "Customer");
            return await IssueToken(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var ok = await _users.CheckPasswordAsync(user, dto.Password);
            if (!ok) return Unauthorized("Invalid credentials");

            return await IssueToken(user);
        }

        private async Task<AuthResponseDto> IssueToken(IdentityUser user)
        {
            var roles = (await _users.GetRolesAsync(user)).ToArray();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:ExpiresMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return new AuthResponseDto(jwt, user.Email ?? "", roles);
        }
    }
}

