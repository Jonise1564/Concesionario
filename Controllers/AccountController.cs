// using Microsoft.AspNetCore.Mvc;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Concesionario.Data;
// using Concesionario.Models;
// using Microsoft.EntityFrameworkCore;
// using BCrypt.Net; 

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
//             // DIAGNÓSTICO 1: Ver qué llega del Frontend
//             Console.WriteLine($"[DIAGNOSTICO LOGIN] -> Frontend envió Username: '{model?.Username}' y Password: '{model?.Password}'");

//             if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
//             {
//                 return BadRequest(new { message = "El usuario y la contraseña son obligatorios." });
//             }

//             // Buscamos TODOS los usuarios activos para ver qué hay en la DB realmente
//             var listaUsuariosDb = await _context.Usuarios.ToListAsync();
//             Console.WriteLine($"[DIAGNOSTICO DB] -> Total de usuarios en la tabla: {listaUsuariosDb.Count}");
//             foreach(var u in listaUsuariosDb)
//             {
//                 Console.WriteLine($"   -> ID: {u.Id} | Username en DB: '{u.NombreUsuario}' | Activo: {u.Activo} | Hash: '{u.Password}'");
//             }

//             // 1. Buscamos el usuario exacto (sin importar mayúsculas/minúsculas)
//             var user = await _context.Usuarios
//                 .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == model.Username.ToLower());

//             if (user == null)
//             {
//                 Console.WriteLine($"[DIAGNOSTICO FALLO] -> No se encontró ningún usuario con el nombre '{model.Username}' en la base de datos.");
//                 return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
//             }

//             if (user.Activo == false)
//             {
//                 Console.WriteLine($"[DIAGNOSTICO FALLO] -> El usuario '{model.Username}' existe pero está DESACTIVADO (Activo = false).");
//                 return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
//             }

//             bool passwordValida = false;

//             // 2. Verificación de contraseña con log detallado
//             try
//             {
//                 if (!string.IsNullOrEmpty(user.Password) && user.Password.StartsWith("$2"))
//                 {
//                     passwordValida = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
//                     Console.WriteLine($"[DIAGNOSTICO CONTRASEÑA] -> ¿Es válida usando BCrypt?: {passwordValida}");
//                 }
//                 else
//                 {
//                     passwordValida = (model.Password == user.Password);
//                     Console.WriteLine($"[DIAGNOSTICO CONTRASEÑA] -> ¿Es válida usando Texto Plano?: {passwordValida} (DB: '{user.Password}' vs Input: '{model.Password}')");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[DIAGNOSTICO ERROR] -> Falló la verificación: {ex.Message}");
//                 passwordValida = (model.Password == user.Password);
//             }

//             if (!passwordValida)
//             {
//                 Console.WriteLine($"[DIAGNOSTICO FALLO] -> La contraseña ingresada no coincide con la guardada.");
//                 return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
//             }

//             Console.WriteLine($"[DIAGNOSTICO ÉXITO] -> Login correcto para '{user.NombreUsuario}'. Generando Token...");

//             // 3. Generación de Token JWT común...
//             var authClaims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//                 new Claim(ClaimTypes.Name, user.NombreUsuario),
//                 new Claim(ClaimTypes.Role, user.Rol), 
//                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//             };

//             var secretKey = _configuration["JWT:Secret"];
//             if (string.IsNullOrEmpty(secretKey))
//             {
//                 return StatusCode(500, new { message = "Error de configuración del servidor (JWT Secret faltante)." });
//             }

//             var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

//             var token = new JwtSecurityToken(
//                 issuer: _configuration["JWT:ValidIssuer"],
//                 audience: _configuration["JWT:ValidAudience"],
//                 expires: DateTime.Now.AddHours(3),
//                 claims: authClaims,
//                 signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
//             );

//             return Ok(new LoginResponse
//             {
//                 Token = new JwtSecurityTokenHandler().WriteToken(token),
//                 Expiration = token.ValidTo,
//                 Username = user.NombreUsuario,
//                 Rol = user.Rol 
//             });
//         }
//     }

//     public class LoginRequest
//     {
//         public string Username { get; set; } = string.Empty;
//         public string Password { get; set; } = string.Empty;
//     }

//     public class LoginResponse
//     {
//         public string Token { get; set; } = string.Empty;
//         public DateTime Expiration { get; set; }
//         public string Username { get; set; } = string.Empty;
//         public string Rol { get; set; } = string.Empty;
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
using BCrypt.Net; 

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
            // DIAGNÓSTICO 1: Ver qué llega del Frontend
            Console.WriteLine($"[DIAGNOSTICO LOGIN] -> Frontend envió Username: '{model?.Username}' y Password: '{model?.Password}'");

            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { message = "El usuario y la contraseña son obligatorios." });
            }

            // Buscamos TODOS los usuarios activos para ver qué hay en la DB realmente
            var listaUsuariosDb = await _context.Usuarios.ToListAsync();
            Console.WriteLine($"[DIAGNOSTICO DB] -> Total de usuarios en la tabla: {listaUsuariosDb.Count}");
            foreach(var u in listaUsuariosDb)
            {
                Console.WriteLine($"   -> ID: {u.Id} | Username en DB: '{u.NombreUsuario}' | Activo: {u.Activo} | Hash: '{u.Password}'");
            }

            // 1. Buscamos el usuario exacto (sin importar mayúsculas/minúsculas)
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == model.Username.ToLower());

            if (user == null)
            {
                Console.WriteLine($"[DIAGNOSTICO FALLO] -> No se encontró ningún usuario con el nombre '{model.Username}' en la base de datos.");
                return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
            }

            if (user.Activo == false)
            {
                Console.WriteLine($"[DIAGNOSTICO FALLO] -> El usuario '{model.Username}' existe pero está DESACTIVADO (Activo = false).");
                return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
            }

            bool passwordValida = false;

            // 2. Verificación de contraseña con log detallado
            try
            {
                if (!string.IsNullOrEmpty(user.Password) && user.Password.StartsWith("$2"))
                {
                    passwordValida = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                    Console.WriteLine($"[DIAGNOSTICO CONTRASEÑA] -> ¿Es válida usando BCrypt?: {passwordValida}");
                }
                else
                {
                    passwordValida = (model.Password == user.Password);
                    Console.WriteLine($"[DIAGNOSTICO CONTRASEÑA] -> ¿Es válida usando Texto Plano?: {passwordValida} (DB: '{user.Password}' vs Input: '{model.Password}')");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DIAGNOSTICO ERROR] -> Falló la verificación: {ex.Message}");
                passwordValida = (model.Password == user.Password);
            }

            if (!passwordValida)
            {
                Console.WriteLine($"[DIAGNOSTICO FALLO] -> La contraseña ingresada no coincide con la guardada.");
                return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta deshabilitada." });
            }

            Console.WriteLine($"[DIAGNOSTICO ÉXITO] -> Login correcto para '{user.NombreUsuario}'. Generando Token...");

            // 3. Generación de Token JWT común...
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                new Claim(ClaimTypes.Role, user.Rol), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var secretKey = _configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(500, new { message = "Error de configuración del servidor (JWT Secret faltante)." });
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Devolvemos la respuesta con el Rol incluido para que el JS sepa qué hacer
            return Ok(new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Username = user.NombreUsuario,
                Rol = user.Rol 
            });
        }
    }

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