using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Concesionario.Data; // Ajustá al namespace de tu ApplicationDbContext
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
        // 📑 LISTAR CLIENTES (Con su Persona vinculada)
        // ---------------------------------------------------------------------
        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Include(c => c.Persona)
                    .OrderByDescending(c => c.Id)
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
            [FromForm] string? ciudad,
            [FromForm] string? estadoProvincia,
            [FromForm] string? codigoPostal,
            [FromForm] string? pais,
            [FromForm] string? calificacionCrediticia,
            [FromForm] string? observaciones)
        {
            try
            {
                // Validaciones de negocio obligatorias (según tus modificadores [Required])
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
                    // Comprobamos si la persona ya existe físicamente en la base de datos por DocumentoIdentidad
                    var personaDb = await _context.Personas.FirstOrDefaultAsync(p => p.DocumentoIdentidad == documentoIdentidad);

                    if (personaDb == null)
                    {
                        // Si no existe, creamos la nueva Persona con todos sus campos
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
                            Ciudad = ciudad?.Trim(),
                            EstadoProvincia = estadoProvincia?.Trim(),
                            CodigoPostal = codigoPostal?.Trim(),
                            Pais = pais?.Trim(),
                            CreadoEl = DateTime.Now,
                            Activo = true
                        };
                        _context.Personas.Add(personaDb);
                        await _context.SaveChangesAsync(); // Guardamos para generar el ID primario de la persona
                    }
                    else
                    {
                        // Si la persona existía previamente, actualizamos su información de perfil por si cambió
                        personaDb.Nombres = nombres.Trim();
                        personaDb.Apellidos = apellidos.Trim();
                        personaDb.Email = email.Trim();
                        personaDb.Telefono = telefono?.Trim();
                        personaDb.TelefonoAlternativo = telefonoAlternativo?.Trim();
                        personaDb.FechaNacimiento = fechaNacimiento;
                        personaDb.Genero = genero?.Trim();
                        personaDb.EstadoCivil = estadoCivil?.Trim();
                        personaDb.Direccion = direccion?.Trim();
                        personaDb.Ciudad = ciudad?.Trim();
                        personaDb.EstadoProvincia = estadoProvincia?.Trim();
                        personaDb.CodigoPostal = codigoPostal?.Trim();
                        personaDb.Pais = pais?.Trim();
                        personaDb.ActualizadoEl = DateTime.Now;
                        
                        _context.Personas.Update(personaDb);
                    }

                    // Validamos que esa persona no esté dada de alta ya en la tabla de clientes comerciales
                    var existeCliente = await _context.Clientes.AnyAsync(c => c.IdPersonaId == personaDb.Id);
                    if (existeCliente) 
                        return BadRequest(new { message = "El documento ingresado ya se encuentra asociado a un cliente existente." });

                    // Creamos el registro comercial en la tabla de clientes
                    var nuevoCliente = new Cliente
                    {
                        IdPersonaId = personaDb.Id,
                        IdFechaAlta = DateTime.Now,
                        CalificacionCrediticia = calificacionCrediticia?.Trim(),
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
                        return NotFound(new { message = "Cliente no encontrado." });

                    // Validamos que si se editó el documento, no colisione con otra persona diferente en la DB
                    if (clienteDb.Persona.DocumentoIdentidad != documentoIdentidad)
                    {
                        var existeDoc = await _context.Personas.AnyAsync(p => p.Id != clienteDb.IdPersonaId && p.DocumentoIdentidad == documentoIdentidad);
                        if (existeDoc) 
                            return BadRequest(new { message = "El documento de identidad ingresado ya pertenece a otro registro." });
                    }

                    // Seteamos las modificaciones de la Persona relacionada
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
                    clienteDb.Persona.Id = clienteDb.Persona.Id; // Preservamos clave
                    clienteDb.Persona.Ciudad = ciudad?.Trim();
                    clienteDb.Persona.EstadoProvincia = estadoProvincia?.Trim();
                    clienteDb.Persona.CodigoPostal = codigoPostal?.Trim();
                    clienteDb.Persona.Pais = pais?.Trim();
                    clienteDb.Persona.ActualizadoEl = DateTime.Now;

                    // Seteamos modificaciones del Cliente comercial
                    clienteDb.CalificacionCrediticia = calificacionCrediticia?.Trim();
                    clienteDb.Observaciones = observaciones?.Trim();

                    _context.Clientes.Update(clienteDb);
                }

                // Impactamos definitivamente todos los cambios de forma conjunta
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