using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Concesionario.Data;
using Concesionario.Models;

namespace Concesionario.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------------
        // 📑 LISTAR CLIENTES (Optimizado con proyección para evitar ciclos de EF Core)
        // ---------------------------------------------------------------------
        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            try
            {
                // Proyectamos directamente lo necesario para romper ciclos relacionales de EF
                var clientes = await _context.Clientes
                    .Include(c => c.Persona)
                        .ThenInclude(p => p.Ciudad)
                            .ThenInclude(ciu => ciu.Provincia)
                    .OrderByDescending(c => c.Id)
                    .Select(c => new
                    {
                        id = c.Id,
                        idPersonaId = c.IdPersonaId,
                        calificacionCrediticia = c.CalificacionCrediticia,
                        idFechaAlta = c.IdFechaAlta,
                        observaciones = c.Observaciones,
                        persona = new
                        {
                            id = c.Persona.Id,
                            documentoIdentidad = c.Persona.DocumentoIdentidad,
                            nombres = c.Persona.Nombres,
                            apellidos = c.Persona.Apellidos,
                            email = c.Persona.Email,
                            telefono = c.Persona.Telefono,
                            telefonoAlternativo = c.Persona.TelefonoAlternativo,
                            fechaNacimiento = c.Persona.FechaNacimiento,
                            genero = c.Persona.Genero,
                            estadoCivil = c.Persona.EstadoCivil,
                            direccion = c.Persona.Direccion,
                            codigoPostal = c.Persona.CodigoPostal,
                            pais = c.Persona.Pais,
                            ciudadId = c.Persona.CiudadId,
                            ciudad = c.Persona.Ciudad != null ? new
                            {
                                id = c.Persona.Ciudad.Id,
                                nombre = c.Persona.Ciudad.Nombre,
                                provincia = c.Persona.Ciudad.Provincia != null ? new
                                {
                                    id = c.Persona.Ciudad.Provincia.Id,
                                    nombre = c.Persona.Ciudad.Provincia.Nombre
                                } : null
                            } : null
                        }
                    })
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener la lista de clientes: " + ex.Message });
            }
        }

        // ---------------------------------------------------------------------
        // 💾 GUARDAR / EDITAR CLIENTE
        // ---------------------------------------------------------------------
        [HttpPost("Guardar")]
        public async Task<IActionResult> Guardar(
            [FromForm] int id,
            [FromForm] int idPersonaId,
            [FromForm] string documentoIdentidad,
            [FromForm] string nombres,
            [FromForm] string apellidos,
            [FromForm] string email,
            [FromForm] string? telefono,
            [FromForm] string? telefonoAlternativo,
            [FromForm] DateTime? fechaNacimiento,
            [FromForm] string? genero,
            [FromForm] string? estadoCivil,
            [FromForm] string? direccion,
            [FromForm] int? ciudadId,
            [FromForm] string? codigoPostal,
            [FromForm] string? pais,
            [FromForm] string? calificacionCrediticia,
            [FromForm] string? observaciones)
        {
            try
            {
                // Validaciones de negocio obligatorias básicas
                if (string.IsNullOrEmpty(documentoIdentidad))
                    return BadRequest(new { message = "El documento de identidad es obligatorio." });
                if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos))
                    return BadRequest(new { message = "Los nombres y apellidos son obligatorios." });
                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { message = "El correo electrónico es obligatorio." });

                documentoIdentidad = documentoIdentidad.Trim();

                // =============================================================
                // 1. LÓGICA PARA NUEVO CLIENTE (id == 0)
                // =============================================================
                if (id == 0)
                {
                    // Comprobamos si la persona ya existe en la DB por DocumentoIdentidad
                    var personaDb = await _context.Personas.FirstOrDefaultAsync(p => p.DocumentoIdentidad == documentoIdentidad);

                    if (personaDb == null)
                    {
                        // Si no existe, instanciamos la nueva Persona asignando su CiudadId
                        personaDb = new Persona
                        {
                            DocumentoIdentidad = documentoIdentidad,
                            Nombres = nombres.Trim(),
                            Apellidos = apellidos.Trim(),
                            Email = email.Trim(),
                            Telefono = telefono?.Trim(),
                            TelefonoAlternativo = telefonoAlternativo?.Trim(),
                            FechaNacimiento = fechaNacimiento,
                            Genero = genero?.Trim(),
                            EstadoCivil = estadoCivil?.Trim(),
                            Direccion = direccion?.Trim(),
                            CiudadId = ciudadId ?? 1, // Fallback preventivo al ID por defecto si llega nulo
                            CodigoPostal = codigoPostal?.Trim(),
                            Pais = pais?.Trim(),
                            CreadoEl = DateTime.Now,
                            Activo = true
                        };
                        _context.Personas.Add(personaDb);
                        await _context.SaveChangesAsync(); // Persistimos para obtener el ID autonumérico
                    }
                    else
                    {
                        // Si la persona ya existía de forma independiente, actualizamos sus datos de contacto
                        personaDb.Nombres = nombres.Trim();
                        personaDb.Apellidos = apellidos.Trim();
                        personaDb.Email = email.Trim();
                        personaDb.Telefono = telefono?.Trim();
                        personaDb.TelefonoAlternativo = telefonoAlternativo?.Trim();
                        personaDb.FechaNacimiento = fechaNacimiento;
                        personaDb.Genero = genero?.Trim();
                        personaDb.EstadoCivil = estadoCivil?.Trim();
                        personaDb.Direccion = direccion?.Trim();
                        personaDb.CiudadId = ciudadId ?? 1;
                        personaDb.CodigoPostal = codigoPostal?.Trim();
                        personaDb.Pais = pais?.Trim();
                        personaDb.ActualizadoEl = DateTime.Now;

                        _context.Personas.Update(personaDb);
                    }

                    // Verificamos que esta persona física no posea ya una ficha comercial activa de cliente
                    var existeCliente = await _context.Clientes.AnyAsync(c => c.IdPersonaId == personaDb.Id);
                    if (existeCliente)
                        return BadRequest(new { message = "El documento ingresado ya se encuentra asociado a un cliente comercial existente." });

                    // Creamos el registro comercial en la tabla de Clientes
                    var nuevoCliente = new Cliente
                    {
                        IdPersonaId = personaDb.Id,
                        IdFechaAlta = DateTime.Now,
                        CalificacionCrediticia = calificacionCrediticia?.Trim() ?? "Buena",
                        Observaciones = observaciones?.Trim()
                    };

                    _context.Clientes.Add(nuevoCliente);
                }
                // =============================================================
                // 2. LÓGICA PARA EDICIÓN DE CLIENTE EXISTENTE (id > 0)
                // =============================================================
                else
                {
                    var clienteDb = await _context.Clientes
                        .Include(c => c.Persona)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (clienteDb == null)
                        return NotFound(new { message = "El perfil de cliente comercial solicitado no existe." });

                    // Validamos la unicidad del documento por si fue alterado en la edición
                    if (clienteDb.Persona.DocumentoIdentidad != documentoIdentidad)
                    {
                        var existeDoc = await _context.Personas.AnyAsync(p => p.Id != clienteDb.IdPersonaId && p.DocumentoIdentidad == documentoIdentidad);
                        if (existeDoc)
                            return BadRequest(new { message = "El documento de identidad ingresado ya se encuentra registrado por otro usuario." });
                    }

                    // Seteamos las modificaciones de la entidad Persona vinculada
                    clienteDb.Persona.DocumentoIdentidad = documentoIdentidad;
                    clienteDb.Persona.Nombres = nombres.Trim();
                    clienteDb.Persona.Apellidos = apellidos.Trim();
                    clienteDb.Persona.Email = email.Trim();
                    clienteDb.Persona.Telefono = telefono?.Trim();
                    clienteDb.Persona.TelefonoAlternativo = telefonoAlternativo?.Trim();
                    clienteDb.Persona.FechaNacimiento = fechaNacimiento;
                    clienteDb.Persona.Genero = genero?.Trim();
                    clienteDb.Persona.EstadoCivil = estadoCivil?.Trim();
                    clienteDb.Persona.Direccion = direccion?.Trim();
                    clienteDb.Persona.CiudadId = ciudadId ?? 1; // Actualización relacional directa numérica
                    clienteDb.Persona.CodigoPostal = codigoPostal?.Trim();
                    clienteDb.Persona.Pais = pais?.Trim();
                    clienteDb.Persona.ActualizadoEl = DateTime.Now;

                    // Modificaciones específicas del registro del Cliente
                    clienteDb.CalificacionCrediticia = calificacionCrediticia?.Trim() ?? "Buena";
                    clienteDb.Observaciones = observaciones?.Trim();

                    _context.Clientes.Update(clienteDb);
                }

                // Confirmamos todos los cambios relacionales en una única transacción atómica
                await _context.SaveChangesAsync();
                return Ok(new { message = "Éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error interno en el servidor al guardar el cliente: " + ex.Message });
            }
        }
    }
}