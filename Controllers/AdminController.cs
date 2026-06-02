using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

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
        // GESTIÓN DE VEHÍCULOS (Bloqueo estricto por Rol)
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
        // APIs GESTIÓN DE CATEGORÍAS
        // ========================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpGet("GetCategorias")]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categorias = await _context.Categorias
                    .Select(c => new { 
                        id = c.Id, 
                        nombre = c.Nombre 
                    })
                    .OrderBy(c => c.nombre)
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener las categorías: " + ex.Message });
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

        // ========================================================
        // GESTIÓN DE VENDEDORES (Corregido a prueba de Nulls)
        // ========================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpGet("GetVendedores")]
        public async Task<IActionResult> GetVendedores()
        {
            try
            {
                var lista = await _context.Vendedores
                    .Include(v => v.Persona)
                    .Include(v => v.Usuario)
                    .Where(v => v.Persona.Activo == true)
                    .Select(v => new VendedorDto
                    {
                        Id = v.Id,
                        DocumentoIdentidad = v.Persona.DocumentoIdentidad ?? "",
                        Nombres = v.Persona.Nombres ?? "",
                        Apellidos = v.Persona.Apellidos ?? "",
                        Email = v.Persona.Email ?? "",
                        Telefono = v.Persona.Telefono ?? "",
                        // Se previene la excepción si la relación Usuario o la columna NombreUsuario son nulas
                        NombreUsuario = v.Usuario != null ? (v.Usuario.NombreUsuario ?? "Sin Usuario") : "Sin Usuario",
                        PorcentajeComision = v.PorcentajeComision,
                        Observaciones = v.Observaciones ?? ""
                    })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener vendedores: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpPost("GuardarVendedor")]
        public async Task<IActionResult> GuardarVendedor([FromBody] VendedorDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Estructura de datos nula." });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (dto.Id == 0)
                    {
                        // ----- ALTA DE VENDEDOR -----
                        var usuarioExiste = await _context.Usuarios
                            .AnyAsync(u => u.NombreUsuario.ToLower() == dto.NombreUsuario.ToLower());
                        
                        if (usuarioExiste)
                            return BadRequest(new { message = "El nombre de usuario ya está registrado en el sistema." });

                        if (string.IsNullOrEmpty(dto.Password))
                            return BadRequest(new { message = "La contraseña es requerida para dar de alta al vendedor." });

                        // 1. Insertar Persona
                        var nuevaPersona = new Persona
                        {
                            DocumentoIdentidad = dto.DocumentoIdentidad,
                            Nombres = dto.Nombres,
                            Apellidos = dto.Apellidos,
                            Email = dto.Email,
                            Telefono = dto.Telefono,
                            CreadoEl = DateTime.Now,
                            Activo = true
                        };
                        _context.Personas.Add(nuevaPersona);
                        await _context.SaveChangesAsync();

                        // 2. Insertar Cuenta de Usuario comercial
                        var nuevoUsuario = new Usuario
                        {
                            NombreUsuario = dto.NombreUsuario,
                            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), 
                            Rol = "Vendedor",
                            Activo = true
                        };
                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

                        // 3. Vincular Entidad Final Vendedor
                        var nuevoVendedor = new Vendedor
                        {
                            IdPersona = nuevaPersona.Id,
                            IdUsuario = nuevoUsuario.Id,
                            FechaContratacion = DateTime.Now,
                            PorcentajeComision = dto.PorcentajeComision,
                            Observaciones = dto.Observaciones
                        };
                        _context.Vendedores.Add(nuevoVendedor);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return Ok(new { message = "Vendedor, Persona y Usuario vinculados con éxito." });
                    }
                    else
                    {
                        // ----- EDICIÓN DE VENDEDOR EXISTENTE -----
                        var vExistente = await _context.Vendedores
                            .Include(v => v.Persona)
                            .Include(v => v.Usuario)
                            .FirstOrDefaultAsync(v => v.Id == dto.Id);

                        if (vExistente == null)
                            return NotFound(new { message = "No se encontró el registro del vendedor." });

                        // Modificar Persona vinculada
                        vExistente.Persona.DocumentoIdentidad = dto.DocumentoIdentidad;
                        vExistente.Persona.Nombres = dto.Nombres;
                        vExistente.Persona.Apellidos = dto.Apellidos;
                        vExistente.Persona.Email = dto.Email;
                        vExistente.Persona.Telefono = dto.Telefono;
                        vExistente.Persona.ActualizadoEl = DateTime.Now;

                        // Modificar Contraseña solo si se llenó en el formulario
                        if (!string.IsNullOrEmpty(dto.Password))
                        {
                            if (vExistente.Usuario != null)
                            {
                                vExistente.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                            }
                        }

                        // Modificar Parámetros de Comisión y Notas
                        vExistente.PorcentajeComision = dto.PorcentajeComision;
                        vExistente.Observaciones = dto.Observaciones;

                        _context.Vendedores.Update(vExistente);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return Ok(new { message = "Vendedor modificado correctamente." });
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Error crítico transaccional: " + ex.Message });
                }
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpDelete("EliminarVendedor")]
        public async Task<IActionResult> EliminarVendedor(int id)
        {
            try
            {
                var vendedor = await _context.Vendedores
                    .Include(v => v.Persona)
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vendedor == null) 
                    return NotFound(new { message = "El vendedor seleccionado no existe." });

                // Borrado lógico para resguardar las relaciones de vehículos comercializados
                vendedor.Persona.Activo = false;
                vendedor.Persona.ActualizadoEl = DateTime.Now;
                
                if (vendedor.Usuario != null)
                {
                    vendedor.Usuario.Activo = false;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Vendedor y accesos suspendidos del sistema." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al dar de baja el registro: " + ex.Message });
            }
        }
    }
}