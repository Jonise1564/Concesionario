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
using MailKit.Net.Smtp;
using MimeKit;
using System.Collections.Generic;

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

        // ========================================================
        // RENDER DE VISTAS (Acceso Web Tradicional)
        // ========================================================

        [AllowAnonymous]
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("Consultas")]
        public IActionResult Consultas()
        {
            return View();
        }

        // ========================================================
        // GESTIÓN DE VEHÍCULOS (Endpoints API protegidos por JWT)
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
                    query = query.Where(v => v.Marca.Contains(filtro) || v.Modelo.Contains(filtro) || v.Patente.Contains(filtro));
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

                if (model.Id == 0)
                {
                    if (!string.IsNullOrEmpty(model.Vin))
                    {
                        var existeVin = await _context.Vehiculos.AnyAsync(v => v.Vin == model.Vin.Trim());
                        if (existeVin) return BadRequest(new { message = "El número de chasis (VIN) ya se encuentra registrado." });
                    }

                    if (!string.IsNullOrEmpty(model.Patente))
                    {
                        var existePatente = await _context.Vehiculos.AnyAsync(v => v.Patente == model.Patente.Trim());
                        if (existePatente) return BadRequest(new { message = "La patente ingresada ya pertenece a otro vehículo en stock." });
                    }

                    model.Vin = model.Vin?.Trim();
                    model.Patente = model.Patente?.Trim();

                    _context.Vehiculos.Add(model);
                }
                else
                {
                    var vehiculoDb = await _context.Vehiculos.FindAsync(model.Id);
                    if (vehiculoDb == null) return NotFound(new { message = "Vehículo no encontrado." });

                    if (!string.IsNullOrEmpty(model.Vin) && vehiculoDb.Vin != model.Vin.Trim())
                    {
                        var existeVin = await _context.Vehiculos.AnyAsync(v => v.Id != model.Id && v.Vin == model.Vin.Trim());
                        if (existeVin) return BadRequest(new { message = "El número de chasis (VIN) ya está en uso." });
                    }

                    if (!string.IsNullOrEmpty(model.Patente) && vehiculoDb.Patente != model.Patente.Trim())
                    {
                        var existePatente = await _context.Vehiculos.AnyAsync(v => v.Id != model.Id && v.Patente == model.Patente.Trim());
                        if (existePatente) return BadRequest(new { message = "La patente ingresada ya está en uso por otro vehículo." });
                    }

                    vehiculoDb.Marca = model.Marca;
                    vehiculoDb.Modelo = model.Modelo;
                    vehiculoDb.Version = model.Version;
                    vehiculoDb.Anio = model.Anio;
                    vehiculoDb.Kilometros = model.Kilometros;
                    vehiculoDb.Precio = model.Precio;
                    vehiculoDb.Combustible = model.Combustible;
                    vehiculoDb.Transmision = model.Transmision;
                    vehiculoDb.CategoriaId = model.CategoriaId;
                    vehiculoDb.Tipo = model.Tipo;
                    vehiculoDb.Activo = model.Activo;
                    vehiculoDb.Vin = model.Vin?.Trim();
                    vehiculoDb.Patente = model.Patente?.Trim();
                    vehiculoDb.Condicion = model.Condicion;
                    vehiculoDb.Estado = model.Estado;

                    if (!string.IsNullOrEmpty(model.ImagenUrl))
                    {
                        vehiculoDb.ImagenUrl = model.ImagenUrl;
                    }

                    _context.Vehiculos.Update(vehiculoDb);
                }

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
        // GESTIÓN DE CATEGORÍAS Y CONDICIONES 
        // ========================================================

        [AllowAnonymous]
        [HttpGet("GetCategorias")]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categories = await _context.Categorias
                    .AsNoTracking()
                    .Select(c => new { id = c.Id, nombre = c.Nombre })
                    .OrderBy(c => c.nombre)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener las categorías: " + ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
        [HttpGet("GetCondiciones")]
        public IActionResult GetCondiciones()
        {
            var condiciones = new List<object>
            {
                new { id = "Nuevo", nombre = "Nuevo" },
                new { id = "Usado", nombre = "Usado" }
            };
            return Ok(condiciones);
        }

        // ========================================================
        // GESTIÓN DE CONSULTAS
        // ========================================================

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

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
            email.To.Add(MailboxAddress.Parse(consulta.Email));
            email.Subject = "Respuesta a tu consulta - Jonel Autos";

            email.Body = new TextPart("plain")
            {
                Text = $"Hola {consulta.Nombre} 👋\n\nRespondimos tu consulta:\n\n{respuesta}\n\nGracias por comunicarte con Jonel Autos 🚗"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("roquerobertomiguellucero@gmail.com", "yxvw pnug qtdv rjvi");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return Ok(new { success = true });
        }

        // ========================================================
        // APIs GESTIÓN DE USUARIOS
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
    }
}