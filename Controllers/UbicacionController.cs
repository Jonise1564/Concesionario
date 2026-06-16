using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Concesionario.Data; 

namespace Concesionario.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")] 
    public class UbicacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UbicacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // RUTA: GET api/Ubicacion/Provincias
        // =====================================================================
        [HttpGet("Provincias")]
        public async Task<IActionResult> GetProvincias()
        {
            try
            {
                // 🛠️ CORRECCIÓN AQUÍ: Accedemos a c.Provincia.Nombre para extraer el texto
                var provincias = await _context.Ciudades
                    .Select(c => c.Provincia.Nombre) 
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(p => new { nombre = p }) 
                    .ToListAsync();

                return Ok(provincias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al mapear las provincias: " + ex.Message });
            }
        }

        // =====================================================================
        // RUTA: GET api/Ubicacion/Ciudades?provincia=San Luis
        // =====================================================================
        [HttpGet("Ciudades")]
        public async Task<IActionResult> GetCiudades([FromQuery] string provincia)
        {
            try
            {
                if (string.IsNullOrEmpty(provincia)) 
                    return BadRequest(new { message = "La provincia es requerida." });

                var ciudades = await _context.Ciudades
                    .Where(c => c.Provincia.Nombre == provincia.Trim())
                    .Select(c => new { id = c.Id, nombre = c.Nombre }) 
                    .OrderBy(c => c.nombre)
                    .ToListAsync();

                return Ok(ciudades);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al mapear las ciudades: " + ex.Message });
            }
        }
    }
}