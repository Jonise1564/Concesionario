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

// //         public AdminController(ApplicationDbContext context)
// //         {
// //             _context = context;
// //         }

// //         // 1. Permitimos entrar a la vista sin el "pop-up" de contraseña
// //         [AllowAnonymous] 
// //         [HttpGet]
// //         [HttpGet("Index")]
// //         public IActionResult Index()
// //         {
// //             return View();
// //         }

// //         // 2. Las APIs siguen PROTEGIDAS. Solo el JS con token podrá leerlas.
// //         [Authorize]
// //         [HttpGet("GetVehiculos")]
// //         public async Task<IActionResult> GetVehiculos()
// //         {
// //             var vehiculos = await _context.Vehiculos.ToListAsync();
// //             return Ok(vehiculos);
// //         }

// //         [Authorize]
// //         [HttpPost("Guardar")]
// //         public async Task<IActionResult> Guardar([FromBody] Vehiculo model)
// //         {
// //             if (model.Id == 0) _context.Vehiculos.Add(model);
// //             else _context.Vehiculos.Update(model);
// //             await _context.SaveChangesAsync();
// //             return Ok(new { message = "Éxito" });
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

//         // API para listar vehículos (Protegida)
//         [Authorize]
//         [HttpGet("GetVehiculos")]
//         public async Task<IActionResult> GetVehiculos()
//         {
//             var vehiculos = await _context.Vehiculos.ToListAsync();
//             return Ok(vehiculos);
//         }

//         // API para Guardar/Modificar con soporte de archivos (Protegida)
//         [Authorize]
//         [HttpPost("Guardar")]
//         public async Task<IActionResult> Guardar([FromForm] Vehiculo model, IFormFile? FotoArchivo)
//         {
//             try
//             {
//                 // Manejo de la subida de imagen física
//                 if (FotoArchivo != null && FotoArchivo.Length > 0)
//                 {
//                     // Crear un nombre único para el archivo
//                     string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(FotoArchivo.FileName);
                    
//                     // Definir la ruta en wwwroot/img/cars
//                     string rutaCarpeta = Path.Combine(_env.WebRootPath, "img", "cars");
                    
//                     if (!Directory.Exists(rutaCarpeta)) 
//                         Directory.CreateDirectory(rutaCarpeta);

//                     string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

//                     // Guardar el archivo en el disco
//                     using (var stream = new FileStream(rutaCompleta, FileMode.Create))
//                     {
//                         await FotoArchivo.CopyToAsync(stream);
//                     }

//                     // Actualizar el modelo con el nuevo nombre de archivo
//                     model.ImagenUrl = nombreArchivo;
//                 }

//                 if (model.Id == 0) 
//                 {
//                     _context.Vehiculos.Add(model);
//                 }
//                 else 
//                 {
//                     // Si no hay foto nueva, model.ImagenUrl conservará 
//                     // el valor enviado desde el campo oculto del formulario.
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

//         // API para Eliminar (Protegida)
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

    



//     }
// }




using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;

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
            // Retornamos todos, activos e inactivos, para que el admin pueda gestionarlos
            var vehiculos = await _context.Vehiculos.ToListAsync();
            return Ok(vehiculos);
        }

        [HttpGet]
            public async Task<IActionResult> GetVehiculos(int pagina = 1, int tamano = 15, string filtro = "")
            {
                var query = _context.Vehiculos.AsQueryable();

                // Filtro on-demand
                if (!string.IsNullOrEmpty(filtro))
                {
                    query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro));
                }

                var total = await query.CountAsync();
                
                // Paginación on-demand
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

                v.Activo = !v.Activo; // Invierte el estado booleano
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
                // Manejo de la subida de imagen física
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
                    // Update rastrea los cambios en el modelo, incluyendo el campo 'Activo' 
                    // que viene desde el checkbox del modal
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
    }
}