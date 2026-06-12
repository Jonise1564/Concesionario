using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Concesionario.Data;
using Concesionario.Models;
using System.Security.Claims;

namespace Concesionario.Controllers
{
    [Route("Admin")] // Mantiene la estructura de rutas /Admin/GetVentas que usás en JS
    [Authorize]      // Protege todo el controlador con tu esquema de tokens JWT
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. OBTENER LISTADO DE VENTAS (JSON)
        // ==========================================
        [HttpGet("GetVentas")]
        public async Task<IActionResult> GetVentas()
        {
            try
            {
                // Traemos las ventas e incluimos las tablas relacionadas
                var listaVentas = await _context.Ventas
                    .Include(v => v.Vehiculo)
                    .Include(v => v.Cliente)
                        .ThenInclude(c => c!.Persona) // Bajamos un nivel para obtener los datos reales de la persona
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                // Mapeamos dinámicamente las propiedades [NotMapped] que lee tu JS
                foreach (var v in listaVentas)
                {
                    if (v.Cliente?.Persona != null)
                    {
                        v.NombreCliente = $"{v.Cliente.Persona.Nombres} {v.Cliente.Persona.Apellidos}";
                    }
                    else
                    {
                        v.NombreCliente = $"Cliente #{v.ClienteId}";
                    }

                    if (v.Vehiculo != null)
                    {
                        v.DetalleVehiculo = $"{v.Vehiculo.Marca} {v.Vehiculo.Modelo} ({v.Vehiculo.Patente})";
                    }
                    else
                    {
                        v.DetalleVehiculo = $"Vehículo #{v.VehiculoId}";
                    }
                }

                return Ok(listaVentas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al recuperar las transacciones", error = ex.Message });
            }
        }

        // ==========================================
        // 2. GUARDAR / REGISTRAR NUEVA VENTA
        // ==========================================
        [HttpPost("GuardarVenta")]
        public async Task<IActionResult> GuardarVenta([FromBody] Venta model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensaje = "Datos inválidos o faltantes en el formulario." });
            }

            // Iniciamos una transacción para asegurar que si falla el cambio de estado del auto, no se guarde la venta
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validar disponibilidad del vehículo
                var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == model.VehiculoId);
                if (vehiculo == null)
                {
                    return NotFound(new { mensaje = "El vehículo seleccionado no existe." });
                }

                // Modificá esto según cómo manejes las cadenas de estados en tu tabla Vehiculos
                if (vehiculo.Estado?.ToLower() == "vendido")
                {
                    return BadRequest(new { mensaje = "Este vehículo ya fue vendido previamente." });
                }

                // 2. Extraer el VendedorId desde el Token JWT de forma automática (si es necesario)
                // Si ya lo mandás calculado en el JSON del JS, podés omitir este bloque.
                if (model.VendedorId == 0)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int idVendedorLogueado))
                    {
                        model.VendedorId = idVendedorLogueado;
                    }
                    else
                    {
                        // Vendedor por defecto de contingencia si el token no expone el ID numérico directo
                        model.VendedorId = 1; 
                    }
                }

                // 3. Procesar Alta (Inserción)
                if (model.Id == 0)
                {
                    // Si el JS mandó la fecha vacía, la seteamos en el servidor
                    if (model.FechaVenta == DateTime.MinValue)
                    {
                        model.FechaVenta = DateTime.Now;
                    }

                    _context.Ventas.Add(model);

                    // 4. Actualizar el estado del coche en stock de forma atómica
                    vehiculo.Estado = "Vendido"; 
                    _context.Vehiculos.Update(vehiculo);
                }
                else
                {
                    // Nota: Las ventas de vehículos por lo general no se editan por auditoría, 
                    // pero si requerís lógica de edición, iría en este bloque.
                    _context.Ventas.Update(model);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync(); // Consolidamos los cambios en MySQL

                return Ok(new { mensaje = "Transacción comercial registrada con éxito." });
            }
            catch (DbUpdateException dbEx)
            {
                await dbTransaction.RollbackAsync();
                // Captura específica por si salta la restricción UNIQUE de la columna VehiculoId
                if (dbEx.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    return BadRequest(new { mensaje = "Error: El vehículo ya se encuentra asignado a otra venta activa." });
                }
                return BadRequest(new { mensaje = "Error de base de datos al procesar la venta.", detalle = dbEx.Message });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return BadRequest(new { mensaje = "No se pudo completar el registro.", error = ex.Message });
            }
        }
    }
}