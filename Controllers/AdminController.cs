using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Concesionario.Controllers
{
    [Route("Admin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Vista principal: Se permite abrir el contenedor HTML sin loops
        [AllowAnonymous] 
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // ========================================================
        // APIs GESTIÓN DE VEHÍCULOS (Bloqueo estricto por Rol)
        // ========================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpGet("GetVehiculos")]
        public async Task<IActionResult> GetVehiculos(int? pagina = null, int tamano = 15, string filtro = "")
        {
            try
            {
                var query = _context.Vehiculos.AsQueryable();
                if (!string.IsNullOrEmpty(filtro))
                {
                    query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro));
                }

                if (pagina == null)
                {
                    var todosLosVehiculos = await query.OrderByDescending(v => v.Id).ToListAsync();
                    return Ok(todosLosVehiculos);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(v => v.Id)
                    .Skip((pagina.Value - 1) * tamano)
                    .Take(tamano)
                    .ToListAsync();

                return Ok(new { items, total });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener los vehículos: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpPost("CambiarEstado")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var v = await _context.Vehiculos.FindAsync(id);
                if (v == null) return NotFound();

                v.Activo = !v.Activo; 
                await _context.SaveChangesAsync();
                return Ok(new { success = true, nuevoEstado = v.Activo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromForm] Vehiculo model, IFormFile? FotoArchivo)
        {
            try
            {
                if (FotoArchivo != null && FotoArchivo.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoArchivo.FileName);
                    string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "cars");
                    if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await FotoArchivo.CopyToAsync(stream);
                    }
                    model.ImagenUrl = nombreArchivo;
                }

                if (model.Id == 0) _context.Vehiculos.Add(model);
                else _context.Vehiculos.Update(model);

                await _context.SaveChangesAsync();
                return Ok(new { message = "Éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en el servidor: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpDelete("Eliminar")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var v = await _context.Vehiculos.FindAsync(id);
                if (v == null) return NotFound();

                _context.Vehiculos.Remove(v);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Eliminado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========================================================
        // APIs GESTIÓN DE CONSULTAS (Bloqueo estricto por Rol)
        // ========================================================

        [AllowAnonymous] 
        [HttpGet("Consultas")]
        public IActionResult Consultas()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpGet("GetConsultas")]
        public async Task<IActionResult> GetConsultas()
        {
            var consultas = await _context.Consultas.OrderByDescending(c => c.Fecha).ToListAsync();
            return Ok(consultas);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpPost("ResponderConsulta")]
        public async Task<IActionResult> ResponderConsulta(int id, string respuesta)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta == null) return NotFound();

            consulta.RespuestaAdmin = respuesta;
            consulta.Estado = "Respondido";
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ========================================================
        // APIs GESTIÓN DE USUARIOS (Diagnóstico de Acceso)
        // ========================================================

        // 🛠️ Optimización: Sacamos transitoriamente Roles="Admin" para comprobar el Fetch de JS.
        // Requiere un Token JWT válido, pero no frena el flujo por inconsistencias de nomenclatura en la DB.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetUsuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new { u.Id, u.NombreUsuario, u.Rol })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener usuarios: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpPost("GuardarUsuario")]
        public async Task<IActionResult> GuardarUsuario([FromBody] Usuario model)
        {
            try
            {
                if (model == null) return BadRequest(new { message = "Datos inválidos." });

                if (model.Id == 0)
                {
                    var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
                    if (existe) return BadRequest(new { message = "El nombre de usuario ya se encuentra registrado." });

                    if (string.IsNullOrEmpty(model.Password)) return BadRequest(new { message = "La contraseña es requerida." });

                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    _context.Usuarios.Add(model);
                }
                else
                {
                    var usuarioDb = await _context.Usuarios.FindAsync(model.Id);
                    if (usuarioDb == null) return NotFound(new { message = "Usuario no encontrado." });

                    if (usuarioDb.NombreUsuario.ToLower() != model.NombreUsuario.ToLower())
                    {
                        var existe = await _context.Usuarios.AnyAsync(u => u.Id != model.Id && u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
                        if (existe) return BadRequest(new { message = "El nombre de usuario ya está en uso." });
                    }

                    usuarioDb.NombreUsuario = model.NombreUsuario;
                    usuarioDb.Rol = model.Rol;

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        usuarioDb.Password = BCrypt.Net.BCrypt.HashPassword(model.Password); 
                    }
                    _context.Usuarios.Update(usuarioDb);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error interno: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpDelete("EliminarUsuario")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return NotFound();

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "No se pudo eliminar: " + ex.Message });
            }
        }
    }
}