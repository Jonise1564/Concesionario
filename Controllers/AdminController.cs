// // using Microsoft.AspNetCore.Authorization;
// // using Microsoft.AspNetCore.Mvc;
// // using Concesionario.Data;
// // using Concesionario.Models;
// // using Microsoft.EntityFrameworkCore;

// // namespace Concesionario.Controllers
// // {
// //     [Route("Admin")]
// //     public class AdminController : Controller
// //     {
// //         private readonly ApplicationDbContext _context;
// //         private readonly IWebHostEnvironment _env;

// //         public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
// //         {
// //             _context = context;
// //             _env = env;
// //         }

// //         // Vista principal del panel
// //         [AllowAnonymous] 
// //         [HttpGet]
// //         [HttpGet("Index")]
// //         public IActionResult Index()
// //         {
// //             return View();
// //         }

// //         // API para listar todos los vehículos (Protegida)
// //         [Authorize]
// //         [HttpGet("GetVehiculos")]
// //         public async Task<IActionResult> GetVehiculos()
// //         {
// //             // Retornamos todos, activos e inactivos, para que el admin pueda gestionarlos
// //             var vehiculos = await _context.Vehiculos.ToListAsync();
// //             return Ok(vehiculos);
// //         }

// //         [HttpGet]
// //         public async Task<IActionResult> GetVehiculos(int pagina = 1, int tamano = 15, string filtro = "")
// //         {
// //             var query = _context.Vehiculos.AsQueryable();

// //             // Filtro on-demand
// //             if (!string.IsNullOrEmpty(filtro))
// //             {
// //                 query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro));
// //             }

// //             var total = await query.CountAsync();
// //             // Paginación on-demand
// //             var items = await query
// //                 .OrderByDescending(v => v.Id)
// //                 .Skip((pagina - 1) * tamano)
// //                 .Take(tamano)
// //                 .ToListAsync();

// //             return Ok(new { items, total });
// //         }

// //         // API para Activar/Desactivar (Borrado Lógico)
// //         [Authorize]
// //         [HttpPost("CambiarEstado")]
// //         public async Task<IActionResult> CambiarEstado(int id)
// //         {
// //             try
// //             {
// //                 var v = await _context.Vehiculos.FindAsync(id);
// //                 if (v == null) return NotFound();

// //                 v.Activo = !v.Activo; // Invierte el estado booleano
// //                 await _context.SaveChangesAsync();

// //                 return Ok(new { success = true, nuevoEstado = v.Activo });
// //             }
// //             catch (Exception ex)
// //             {
// //                 return BadRequest(new { message = ex.Message });
// //             }
// //         }

// //         // API para Guardar/Modificar con soporte de archivos (Protegida)
// //         [Authorize]
// //         [HttpPost("Guardar")]
// //         public async Task<IActionResult> Guardar([FromForm] Vehiculo model, IFormFile? FotoArchivo)
// //         {
// //             try
// //             {
// //                 // Manejo de la subida de imagen física
// //                 if (FotoArchivo != null && FotoArchivo.Length > 0)
// //                 {
// //                     string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoArchivo.FileName);
// //                     string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "cars");
// //                     if (!Directory.Exists(rutaCarpeta)) 
// //                         Directory.CreateDirectory(rutaCarpeta);

// //                     string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

// //                     using (var stream = new FileStream(rutaCompleta, FileMode.Create))
// //                     {
// //                         await FotoArchivo.CopyToAsync(stream);
// //                     }

// //                     model.ImagenUrl = nombreArchivo;
// //                 }

// //                 if (model.Id == 0) 
// //                 {
// //                     _context.Vehiculos.Add(model);
// //                 }
// //                 else 
// //                 {
// //                     // Update rastrea los cambios en el modelo, incluyendo el campo 'Activo' 
// //                     // que viene desde el checkbox del modal
// //                     _context.Vehiculos.Update(model);
// //                 }

// //                 await _context.SaveChangesAsync();
// //                 return Ok(new { message = "Éxito" });
// //             }
// //             catch (Exception ex)
// //             {
// //                 return BadRequest(new { message = "Error en el servidor: " + ex.Message });
// //             }
// //         }

// //         // API para Eliminar definitivamente (Protegida)
// //         [Authorize]
// //         [HttpDelete("Eliminar")]
// //         public async Task<IActionResult> Eliminar(int id)
// //         {
// //             try
// //             {
// //                 var v = await _context.Vehiculos.FindAsync(id);
// //                 if (v == null) return NotFound();

// //                 _context.Vehiculos.Remove(v);
// //                 await _context.SaveChangesAsync();
// //                 return Ok(new { message = "Eliminado" });
// //             }
// //             catch (Exception ex)
// //             {
// //                 return BadRequest(new { message = ex.Message });
// //             }
// //         }

// //         // =========================
// //         // CONSULTAS
// //         // =========================

// //         [AllowAnonymous]
// //         [HttpGet("Consultas")]
// //         public IActionResult Consultas()
// //         {
// //             return View();
// //         }

// //         [Authorize]
// //         [HttpGet("GetConsultas")]
// //         public async Task<IActionResult> GetConsultas()
// //         {
// //             var consultas = await _context.Consultas
// //                 .OrderByDescending(c => c.Fecha)
// //                 .ToListAsync();

// //             return Ok(consultas);
// //         }

// //         [Authorize]
// //         [HttpPost("ResponderConsulta")]
// //         public async Task<IActionResult> ResponderConsulta(int id, string respuesta)
// //         {
// //             var consulta = await _context.Consultas.FindAsync(id);

// //             if (consulta == null)
// //                 return NotFound();

// //             consulta.RespuestaAdmin = respuesta;
// //             consulta.Estado = "Respondido";

// //             await _context.SaveChangesAsync();

// //             return Ok(new { success = true });
// //         }
// //     }
// // }


// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Concesionario.Data;
// using Concesionario.Models;
// using Microsoft.EntityFrameworkCore;

// namespace Concesionario.Controllers
// {
//     [Route("Admin")]
//     public class AdminController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IWebHostEnvironment _env;

//         public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
//         {
//             _context = context;
//             _env = env;
//         }

//         // Vista principal del panel
//         [AllowAnonymous] 
//         [HttpGet]
//         [HttpGet("Index")]
//         public IActionResult Index()
//         {
//             return View();
//         }

//         // API para listar todos los vehículos (Protegida)
//         [Authorize]
//         [HttpGet("GetVehiculos")]
//         public async Task<IActionResult> GetVehiculos()
//         {
//             var vehiculos = await _context.Vehiculos.ToListAsync();
//             return Ok(vehiculos);
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetVehiculos(int pagina = 1, int tamano = 15, string filtro = "")
//         {
//             var query = _context.Vehiculos.AsQueryable();

//             if (!string.IsNullOrEmpty(filtro))
//             {
//                 query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro));
//             }

//             var total = await query.CountAsync();
//             var items = await query
//                 .OrderByDescending(v => v.Id)
//                 .Skip((pagina - 1) * tamano)
//                 .Take(tamano)
//                 .ToListAsync();

//             return Ok(new { items, total });
//         }

//         // API para Activar/Desactivar (Borrado Lógico)
//         [Authorize]
//         [HttpPost("CambiarEstado")]
//         public async Task<IActionResult> CambiarEstado(int id)
//         {
//             try
//             {
//                 var v = await _context.Vehiculos.FindAsync(id);
//                 if (v == null) return NotFound();

//                 v.Activo = !v.Activo; 
//                 await _context.SaveChangesAsync();

//                 return Ok(new { success = true, nuevoEstado = v.Activo });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         // API para Guardar/Modificar con soporte de archivos (Protegida)
//         [Authorize]
//         [HttpPost("Guardar")]
//         public async Task<IActionResult> Guardar([FromForm] Vehiculo model, IFormFile? FotoArchivo)
//         {
//             try
//             {
//                 if (FotoArchivo != null && FotoArchivo.Length > 0)
//                 {
//                     string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoArchivo.FileName);
//                     string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "cars");
//                     if (!Directory.Exists(rutaCarpeta)) 
//                         Directory.CreateDirectory(rutaCarpeta);

//                     string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

//                     using (var stream = new FileStream(rutaCompleta, FileMode.Create))
//                     {
//                         await FotoArchivo.CopyToAsync(stream);
//                     }

//                     model.ImagenUrl = nombreArchivo;
//                 }

//                 if (model.Id == 0) 
//                 {
//                     _context.Vehiculos.Add(model);
//                 }
//                 else 
//                 {
//                     _context.Vehiculos.Update(model);
//                 }

//                 await _context.SaveChangesAsync();
//                 return Ok(new { message = "Éxito" });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = "Error en el servidor: " + ex.Message });
//             }
//         }

//         // API para Eliminar definitivamente (Protegida)
//         [Authorize]
//         [HttpDelete("Eliminar")]
//         public async Task<IActionResult> Eliminar(int id)
//         {
//             try
//             {
//                 var v = await _context.Vehiculos.FindAsync(id);
//                 if (v == null) return NotFound();

//                 _context.Vehiculos.Remove(v);
//                 await _context.SaveChangesAsync();
//                 return Ok(new { message = "Eliminado" });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         // =========================
//         // CONSULTAS
//         // =========================

//         [AllowAnonymous]
//         [HttpGet("Consultas")]
//         public IActionResult Consultas()
//         {
//             return View();
//         }

//         [Authorize]
//         [HttpGet("GetConsultas")]
//         public async Task<IActionResult> GetConsultas()
//         {
//             var consultas = await _context.Consultas
//                 .OrderByDescending(c => c.Fecha)
//                 .ToListAsync();

//             return Ok(consultas);
//         }

//         [Authorize]
//         [HttpPost("ResponderConsulta")]
//         public async Task<IActionResult> ResponderConsulta(int id, string respuesta)
//         {
//             var consulta = await _context.Consultas.FindAsync(id);

//             if (consulta == null)
//                 return NotFound();

//             consulta.RespuestaAdmin = respuesta;
//             consulta.Estado = "Respondido";

//             await _context.SaveChangesAsync();

//             return Ok(new { success = true });
//         }

//         // ========================================================
//         // NUEVO: GESTIÓN DE USUARIOS (Conectado con tu Partial JS)
//         // ========================================================

//         // 1. Obtener todos los usuarios de la DB
//         [Authorize]
//         [HttpGet("GetUsuarios")]
//         public async Task<IActionResult> GetUsuarios()
//         {
//             try
//             {
//                 // Traemos los usuarios omitiendo la contraseña por seguridad en el JSON
//                 var usuarios = await _context.Usuarios
//                     .Select(u => new {
//                         u.Id,
//                         u.NombreUsuario,
//                         u.Rol
//                     })
//                     .ToListAsync();

//                 return Ok(usuarios);
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = "Error al obtener usuarios: " + ex.Message });
//             }
//         }

//         // 2. Guardar o Modificar Usuario
//         [Authorize]
//         [HttpPost("GuardarUsuario")]
//         public async Task<IActionResult> GuardarUsuario([FromBody] Usuario model)
//         {
//             try
//             {
//                 if (model == null) return BadRequest(new { message = "Datos inválidos." });

//                 // MODO: CREAR NUEVO
//                 if (model.Id == 0)
//                 {
//                     // Validamos que el nombre no esté repetido
//                     var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
//                     if (existe) return BadRequest(new { message = "El nombre de usuario ya se encuentra registrado." });

//                     if (string.IsNullOrEmpty(model.Password)) 
//                         return BadRequest(new { message = "La contraseña es requerida para un nuevo usuario." });

//                     // Opcional: BCrypt.Net.BCrypt.HashPassword(model.Password) si usás hashing
//                     // Por ahora se guarda tal cual lo recibe el modelo asignado
//                     _context.Usuarios.Add(model);
//                 }
//                 // MODO: MODIFICAR EXISTENTE
//                 else
//                 {
//                     var usuarioDb = await _context.Usuarios.FindAsync(model.Id);
//                     if (usuarioDb == null) return NotFound(new { message = "Usuario no encontrado." });

//                     // Validar si cambió el nombre y el nuevo ya existe
//                     if (usuarioDb.NombreUsuario.ToLower() != model.NombreUsuario.ToLower())
//                     {
//                         var existe = await _context.Usuarios.AnyAsync(u => u.Id != model.Id && u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
//                         if (existe) return BadRequest(new { message = "El nombre de usuario ya está en uso." });
//                     }

//                     usuarioDb.NombreUsuario = model.NombreUsuario;
//                     usuarioDb.Rol = model.Rol;

//                     // Si escribió algo en el campo Password, se la cambiamos. Si no, queda la que estaba
//                     if (!string.IsNullOrEmpty(model.Password))
//                     {
//                         usuarioDb.Password = model.Password; 
//                     }

//                     _context.Usuarios.Update(usuarioDb);
//                 }

//                 await _context.SaveChangesAsync();
//                 return Ok(new { message = "Éxito" });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = "Error interno: " + ex.Message });
//             }
//         }

//         // 3. Eliminar Usuario definitivamente
//         [Authorize]
//         [HttpDelete("EliminarUsuario")]
//         public async Task<IActionResult> EliminarUsuario(int id)
//         {
//             try
//             {
//                 var usuario = await _context.Usuarios.FindAsync(id);
//                 if (usuario == null) return NotFound();

//                 _context.Usuarios.Remove(usuario);
//                 await _context.SaveChangesAsync();

//                 return Ok(new { message = "Usuario eliminado correctamente" });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new { message = "No se pudo eliminar: " + ex.Message });
//             }
//         }
//     }
// }








using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // <-- Asegura el uso de BCrypt.Net-Next

namespace Concesionario.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Vista principal del panel
        [AllowAnonymous] 
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // API para listar todos los vehículos (Protegida)
        [Authorize]
        [HttpGet("GetVehiculos")]
        public async Task<IActionResult> GetVehiculos()
        {
            var vehiculos = await _context.Vehiculos.ToListAsync();
            return Ok(vehiculos);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiculos(int pagina = 1, int tamano = 15, string filtro = "")
        {
            var query = _context.Vehiculos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(v => v.Id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return Ok(new { items, total });
        }

        // API para Activar/Desactivar (Borrado Lógico)
        [Authorize]
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

        // API para Guardar/Modificar con soporte de archivos (Protegida)
        [Authorize]
        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromForm] Vehiculo model, IFormFile? FotoArchivo)
        {
            try
            {
                if (FotoArchivo != null && FotoArchivo.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoArchivo.FileName);
                    string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "cars");
                    if (!Directory.Exists(rutaCarpeta)) 
                        Directory.CreateDirectory(rutaCarpeta);

                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await FotoArchivo.CopyToAsync(stream);
                    }

                    model.ImagenUrl = nombreArchivo;
                }

                if (model.Id == 0) 
                {
                    _context.Vehiculos.Add(model);
                }
                else 
                {
                    _context.Vehiculos.Update(model);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en el servidor: " + ex.Message });
            }
        }

        // API para Eliminar definitivamente (Protegida)
        [Authorize]
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

        // =========================
        // CONSULTAS
        // =========================

        [AllowAnonymous]
        [HttpGet("Consultas")]
        public IActionResult Consultas()
        {
            return View();
        }

        [Authorize]
        [HttpGet("GetConsultas")]
        public async Task<IActionResult> GetConsultas()
        {
            var consultas = await _context.Consultas
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return Ok(consultas);
        }

        [Authorize]
        [HttpPost("ResponderConsulta")]
        public async Task<IActionResult> ResponderConsulta(int id, string respuesta)
        {
            var consulta = await _context.Consultas.FindAsync(id);

            if (consulta == null)
                return NotFound();

            consulta.RespuestaAdmin = respuesta;
            consulta.Estado = "Respondido";

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ========================================================
        // GESTIÓN DE USUARIOS
        // ========================================================

        // 1. Obtener todos los usuarios de la DB
        [Authorize]
        [HttpGet("GetUsuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new {
                        u.Id,
                        u.NombreUsuario,
                        u.Rol
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener usuarios: " + ex.Message });
            }
        }

        // 2. Guardar o Modificar Usuario (Con BCrypt Hashing)
        [Authorize]
        [HttpPost("GuardarUsuario")]
        public async Task<IActionResult> GuardarUsuario([FromBody] Usuario model)
        {
            try
            {
                if (model == null) return BadRequest(new { message = "Datos inválidos." });

                // MODO: CREAR NUEVO
                if (model.Id == 0)
                {
                    var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.ToLower());
                    if (existe) return BadRequest(new { message = "El nombre de usuario ya se encuentra registrado." });

                    if (string.IsNullOrEmpty(model.Password)) 
                        return BadRequest(new { message = "La contraseña es requerida para un nuevo usuario." });

                    // Hasheamos la contraseña con BCrypt antes de guardar en la DB
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    _context.Usuarios.Add(model);
                }
                // MODO: MODIFICAR EXISTENTE
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

                    // Si el administrador ingresó una contraseña, la hasheamos y modificamos
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

        // 3. Eliminar Usuario definitivamente
        [Authorize]
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
