// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Concesionario.Data;
// using Concesionario.Models;
// using Microsoft.EntityFrameworkCore;

// namespace Concesionario.Controllers
// {
//     [Authorize] // Solo permite acceso si el usuario está autenticado vía JWT
//     [Route("Admin")] // Define que la URL base para este controlador es /Admin
//     public class AdminController : Controller
//     {
//         private readonly ApplicationDbContext _context;

//         public AdminController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // 1. Carga la página principal del Panel
//         // Acceso: GET /Admin o /Admin/Index
//         [HttpGet]
//         [HttpGet("Index")]
//         public IActionResult Index()
//         {
//             // Busca automáticamente la carpeta Views/Admin/Index.cshtml
//             return View();
//         }

//         // 2. API para obtener la lista de autos
//         // Acceso: GET /Admin/GetVehiculos
//         [HttpGet("GetVehiculos")]
//         public async Task<IActionResult> GetVehiculos()
//         {
//             var vehiculos = await _context.Vehiculos.ToListAsync();
//             return Ok(vehiculos);
//         }

//         // 3. API para Guardar o Editar
//         // Acceso: POST /Admin/Guardar
//         [HttpPost("Guardar")]
//         public async Task<IActionResult> Guardar([FromBody] Vehiculo model)
//         {
//             if (model == null) return BadRequest("Datos inválidos");

//             if (model.Id == 0)
//                 _context.Vehiculos.Add(model);
//             else
//                 _context.Vehiculos.Update(model);

//             await _context.SaveChangesAsync();
//             return Ok(new { message = "Operación exitosa" });
//         }

//         // 4. API para Eliminar
//         // Acceso: DELETE /Admin/Eliminar/{id}
//         [HttpDelete("Eliminar")]
//         public async Task<IActionResult> Eliminar(int id)
//         {
//             var vehiculo = await _context.Vehiculos.FindAsync(id);
//             if (vehiculo == null) return NotFound();

//             _context.Vehiculos.Remove(vehiculo);
//             await _context.SaveChangesAsync();
//             return Ok(new { message = "Vehículo eliminado" });
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

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Permitimos entrar a la vista sin el "pop-up" de contraseña
        [AllowAnonymous] 
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        // 2. Las APIs siguen PROTEGIDAS. Solo el JS con token podrá leerlas.
        [Authorize]
        [HttpGet("GetVehiculos")]
        public async Task<IActionResult> GetVehiculos()
        {
            var vehiculos = await _context.Vehiculos.ToListAsync();
            return Ok(vehiculos);
        }

        [Authorize]
        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] Vehiculo model)
        {
            if (model.Id == 0) _context.Vehiculos.Add(model);
            else _context.Vehiculos.Update(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Éxito" });
        }
    }
}