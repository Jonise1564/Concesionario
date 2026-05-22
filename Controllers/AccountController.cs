// using Microsoft.AspNetCore.Mvc;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Concesionario.Data;
// using Concesionario.Models;
// using Microsoft.EntityFrameworkCore;


// namespace Concesionario.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AccountController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IConfiguration _configuration;

//         public AccountController(ApplicationDbContext context, IConfiguration configuration)
//         {
//             _context = context;
//             _configuration = configuration;
//         }

//         [HttpPost("login")]
//         public async Task<IActionResult> Login([FromBody] LoginRequest model)
//         {
//             // 1. Buscamos el usuario en la base de datos MySQL
//             var user = await _context.Usuarios
//                 .FirstOrDefaultAsync(u => u.NombreUsuario == model.Username && u.Password == model.Password);

//             if (user != null)
//             {
//                 // 2. Creamos los permisos (Claims)
//                 var authClaims = new List<Claim>
//                 {
//                     new Claim(ClaimTypes.Name, user.NombreUsuario),
//                     new Claim(ClaimTypes.Role, user.Rol), // "Admin" o "Vendedor"
//                     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                 };

//                 // 3. Obtenemos la clave secreta desde appsettings.json
//                 var authSigningKey = new SymmetricSecurityKey(
//                     Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

//                 // 4. Generamos el Token
//                 var token = new JwtSecurityToken(
//                     issuer: _configuration["JWT:ValidIssuer"],
//                     audience: _configuration["JWT:ValidAudience"],
//                     expires: DateTime.Now.AddHours(3), // El token dura 3 horas
//                     claims: authClaims,
//                     signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
//                 );

//                 return Ok(new LoginResponse
//                 {
//                     Token = new JwtSecurityTokenHandler().WriteToken(token),
//                     Expiration = token.ValidTo,
//                     Username = user.NombreUsuario
//                 });
//             }

//             // Si el usuario no existe o la clave es incorrecta
//             return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
//         }
//     }
// }



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
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { message = "El usuario y la contraseña son obligatorios." });
            }

            // 1. Buscamos el usuario en MySQL validando que esté ACTIVO (Blindaje para el borrado lógico)
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == model.Username 
                                       && u.Password == model.Password 
                                       && u.Activo == true); // <-- Validamos la baja lógica

            if (user != null)
            {
                // 2. Creamos los permisos (Claims) incluyendo su ID y Rol
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.NombreUsuario),
                    new Claim(ClaimTypes.Role, user.Rol), // "Admin" o "Vendedor"
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // 3. Obtenemos la clave secreta desde appsettings.json
                var secretKey = _configuration["JWT:Secret"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    return StatusCode(500, new { message = "Error de configuración del servidor (JWT Secret faltante)." });
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                // 4. Generamos el Token JWT (Expira en 3 horas)
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // 5. Respuesta exitosa con el Token armado
                return Ok(new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    Username = user.NombreUsuario,
                    Rol = user.Rol // Agregado para que el frontend sepa qué menú mostrar
                });
            }

            // Si el usuario no existe, la clave es incorrecta o está desactivado (Activo == false)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
        }
    }

    // ==========================================================
    //  MODELOS DTO (Data Transfer Objects) PARA EL LOGIN
    // ==========================================================
    
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}