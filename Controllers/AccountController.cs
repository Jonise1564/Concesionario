using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;


namespace Concesionario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            // 1. Buscamos el usuario en la base de datos MySQL
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == model.Username && u.Password == model.Password);

            if (user != null)
            {
                // 2. Creamos los permisos (Claims)
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.NombreUsuario),
                    new Claim(ClaimTypes.Role, user.Rol), // "Admin" o "Vendedor"
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // 3. Obtenemos la clave secreta desde appsettings.json
                var authSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                // 4. Generamos el Token
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3), // El token dura 3 horas
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    Username = user.NombreUsuario
                });
            }

            // Si el usuario no existe o la clave es incorrecta
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }
    }
}