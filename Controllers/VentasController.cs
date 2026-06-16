using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Concesionario.Data;
using Concesionario.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Concesionario.Controllers
{
    [Authorize]      
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. OBTENER LISTADO DE VENTAS (JSON)
        // Ruta explícita: /Admin/GetVentas
        // ==========================================
        [HttpGet]
        [Route("Admin/GetVentas")]
        public async Task<IActionResult> GetVentas()
        {
            try
            {
                var listaVentas = await _context.Ventas
                    .Include(v => v.Cliente)
                        .ThenInclude(c => c!.Persona) 
                    .Include(v => v.FormaPago)          
                    .Include(v => v.TipoComprobante)    
                    .Include(v => v.DetallesVenta)      
                        .ThenInclude(d => d.Vehiculo)   
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                var resultado = listaVentas.Select(v => new {
                    id = v.Id,
                    clienteId = v.ClienteId,
                    vendedorId = v.VendedorId,
                    tipoComprobanteId = v.TipoComprobanteId,
                    formaPagoId = v.FormaPagoId,
                    puntoVenta = v.PuntoVenta,
                    nroComprobante = v.NroComprobante,
                    fechaVenta = v.FechaVenta,
                    montoFinal = v.MontoFinal,
                    observaciones = v.Observaciones,
                    nombreCliente = v.Cliente?.Persona != null 
                        ? $"{v.Cliente.Persona.Nombres} {v.Cliente.Persona.Apellidos}" 
                        : $"Cliente #{v.ClienteId}",
                    formaPago = v.FormaPago?.Nombre ?? $"ID {v.FormaPagoId}",
                    tipoComprobante = v.TipoComprobante?.Nombre ?? $"ID {v.TipoComprobanteId}",
                    
                    detalles = v.DetallesVenta.Select(d => new {
                        id = d.Id,
                        vehiculoId = d.VehiculoId,
                        repuestoId = d.RepuestoId,
                        servicioId = d.ServicioId,
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioUnitario,
                        descripcionItem = d.Vehiculo != null 
                            ? $"Vehículo: {d.Vehiculo.Marca} {d.Vehiculo.Modelo} ({d.Vehiculo.Patente})"
                            : d.RepuestoId != null ? $"Repuesto #{d.RepuestoId}" 
                            : d.ServicioId != null ? $"Servicio #{d.ServicioId}" 
                            : "Ítems de venta masiva"
                    })
                });

                // Forzamos formato JSON nativo con propiedades en minúscula (camelCase) para Javascript
                return Json(resultado);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Error al recuperar las transacciones", error = ex.Message });
            }
        }

        // ==========================================
        // 2. GUARDAR / REGISTRAR NUEVA VENTA (MAESTRO-DETALLE)
        // Ruta explícita: /Admin/GuardarVenta
        // ==========================================
        [HttpPost]
        [Route("Admin/GuardarVenta")]
        public async Task<IActionResult> GuardarVenta([FromBody] Venta model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Datos inválidos o faltantes en el formulario." });
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (model.VendedorId == 0)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int idVendedorLogueado))
                    {
                        model.VendedorId = idVendedorLogueado;
                    }
                    else
                    {
                        model.VendedorId = 1; 
                    }
                }

                if (model.FechaVenta == DateTime.MinValue)
                {
                    model.FechaVenta = DateTime.Now;
                }

                if (model.Id == 0)
                {
                    if (model.DetallesVenta == null || !model.DetallesVenta.Any())
                    {
                        Response.StatusCode = 400;
                        return Json(new { mensaje = "No se puede registrar una venta sin ítems en el detalle." });
                    }

                    foreach (var detalle in model.DetallesVenta)
                    {
                        if (detalle.VehiculoId.HasValue && detalle.VehiculoId.Value > 0)
                        {
                            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(veh => veh.Id == detalle.VehiculoId.Value);
                            if (vehiculo == null)
                            {
                                Response.StatusCode = 404;
                                return Json(new { mensaje = $"El vehículo con ID {detalle.VehiculoId} no existe." });
                            }

                            if (vehiculo.Estado?.ToLower() == "vendido")
                            {
                                Response.StatusCode = 400;
                                return Json(new { mensaje = $"El vehículo {vehiculo.Marca} {vehiculo.Modelo} ({vehiculo.Patente}) ya fue vendido." });
                            }

                            vehiculo.Estado = "Vendido";
                            _context.Vehiculos.Update(vehiculo);
                        }
                    }

                    _context.Ventas.Add(model);
                }
                else
                {
                    _context.Ventas.Update(model);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync(); 

                return Json(new { mensaje = "Transacción comercial registrada con éxito.", ventaId = model.Id });
            }
            catch (DbUpdateException dbEx)
            {
                await dbTransaction.RollbackAsync();
                Response.StatusCode = 400;
                if (dbEx.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    return Json(new { mensaje = "Error: El número de comprobante o el ítem ya se encuentra registrado." });
                }
                return Json(new { mensaje = "Error de base de datos al procesar la venta.", detalle = dbEx.Message });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                Response.StatusCode = 400;
                return Json(new { mensaje = "No se pudo completar el registro.", error = ex.Message });
            }
        }
    }
}