using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Concesionario.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin")]
    [ApiController]
    [Route("Admin")] 
    public class VendedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VendedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

            // GET: /Admin/GetVendedores
        [HttpGet("GetVendedores")]
        public async Task<IActionResult> GetVendedores()
        {
            try
            {
                var lista = await _context.Vendedores
                    .Include(v => v.Persona)
                        .ThenInclude(p => p.Ciudad)
                            .ThenInclude(c => c.Provincia)
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
                        FechaNacimiento = v.Persona.FechaNacimiento,
                        Genero = v.Persona.Genero ?? "",
                        EstadoCivil = v.Persona.EstadoCivil ?? "",
                        
                        // 🗺️ CORREGIDO: Mapeo exacto según las propiedades de tu VendedorDto
                        CiudadId = v.Persona.CiudadId,
                        NombreCiudad = v.Persona.Ciudad != null ? v.Persona.Ciudad.Nombre : "",
                        Provincia = v.Persona.Ciudad != null && v.Persona.Ciudad.Provincia != null ? v.Persona.Ciudad.Provincia.Nombre : "",
                        
                        CodigoPostal = v.Persona.CodigoPostal ?? "",
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

        // POST: /Admin/GuardarVendedor
        [HttpPost("GuardarVendedor")]
        public async Task<IActionResult> GuardarVendedor([FromBody] VendedorDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Estructura de datos nula." });
            if (string.IsNullOrEmpty(dto.Email)) return BadRequest(new { message = "El Correo Electrónico es un campo obligatorio." });

            if (dto.FechaNacimiento.HasValue)
            {
                var fechaNac = dto.FechaNacimiento.Value;
                var hoy = DateTime.Today;
                int edad = hoy.Year - fechaNac.Year;
                if (fechaNac.Date > hoy.AddYears(-edad)) edad--;

                if (edad < 18 || edad >= 70) return BadRequest(new { message = "El vendedor debe tener entre 18 y 69 años de edad." });
            }
            else
            {
                return BadRequest(new { message = "La fecha de nacimiento es un campo obligatorio." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (dto.Id == 0)
                    {
                        var dniExiste = await _context.Personas.AnyAsync(p => p.DocumentoIdentidad == dto.DocumentoIdentidad.Trim() && p.Activo == true);
                        if (dniExiste) return BadRequest(new { message = "El Documento de Identidad (DNI) ya pertenece a un vendedor registrado." });

                        var emailExiste = await _context.Personas.AnyAsync(p => p.Email.ToLower() == dto.Email.Trim().ToLower() && p.Activo == true);
                        if (emailExiste) return BadRequest(new { message = "El Correo Electrónico ya se encuentra registrado." });

                        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == dto.NombreUsuario.Trim().ToLower());
                        if (usuarioExiste) return BadRequest(new { message = "El Nombre de Usuario ya se encuentra registrado." });

                        if (string.IsNullOrEmpty(dto.Password)) return BadRequest(new { message = "La contraseña es requerida para el alta." });

                        var nuevaPersona = new Persona
                        {
                            DocumentoIdentidad = dto.DocumentoIdentidad.Trim(),
                            Nombres = dto.Nombres.Trim(),
                            Apellidos = dto.Apellidos.Trim(),
                            Email = dto.Email.Trim(),
                            Telefono = dto.Telefono?.Trim(),
                            FechaNacimiento = dto.FechaNacimiento,
                            Genero = dto.Genero,
                            EstadoCivil = dto.EstadoCivil,
                            CiudadId = dto.CiudadId ?? 1, // Tomas el id enviado o usas el fallback 1
                            CodigoPostal = dto.CodigoPostal,
                            CreadoEl = DateTime.Now,
                            Activo = true
                        };
                        _context.Personas.Add(nuevaPersona);
                        await _context.SaveChangesAsync();

                        var nuevoUsuario = new Usuario
                        {
                            NombreUsuario = dto.NombreUsuario.Trim(),
                            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                            Rol = "Vendedor",
                            Activo = true
                        };
                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

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
                        var vExistente = await _context.Vendedores
                            .Include(v => v.Persona)
                            .Include(v => v.Usuario)
                            .FirstOrDefaultAsync(v => v.Id == dto.Id);

                        if (vExistente == null) return NotFound(new { message = "No se encontró el registro del vendedor." });

                        var dniDuplicado = await _context.Personas
                            .AnyAsync(p => p.Id != vExistente.IdPersona && p.DocumentoIdentidad == dto.DocumentoIdentidad.Trim() && p.Activo == true);
                        if (dniDuplicado) return BadRequest(new { message = "El DNI ingresado ya pertenece a otro vendedor activo." });

                        var emailDuplicado = await _context.Personas
                            .AnyAsync(p => p.Id != vExistente.IdPersona && p.Email.ToLower() == dto.Email.Trim().ToLower() && p.Activo == true);
                        if (emailDuplicado) return BadRequest(new { message = "El Correo Electrónico ya está en uso por otro usuario." });

                        if (vExistente.Usuario != null)
                        {
                            if (vExistente.Usuario.NombreUsuario.ToLower() != dto.NombreUsuario.Trim().ToLower())
                            {
                                var usuarioDuplicado = await _context.Usuarios
                                    .AnyAsync(u => u.Id != vExistente.IdUsuario && u.NombreUsuario.ToLower() == dto.NombreUsuario.Trim().ToLower());
                                if (usuarioDuplicado) return BadRequest(new { message = "El Nombre de Usuario ingresado ya está en uso." });

                                vExistente.Usuario.NombreUsuario = dto.NombreUsuario.Trim();
                            }

                            if (!string.IsNullOrEmpty(dto.Password))
                            {
                                vExistente.Usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                            }
                        }

                        vExistente.Persona.DocumentoIdentidad = dto.DocumentoIdentidad.Trim();
                        vExistente.Persona.Nombres = dto.Nombres.Trim();
                        vExistente.Persona.Apellidos = dto.Apellidos.Trim();
                        vExistente.Persona.Email = dto.Email.Trim();
                        vExistente.Persona.Telefono = dto.Telefono?.Trim();
                        vExistente.Persona.FechaNacimiento = dto.FechaNacimiento;
                        vExistente.Persona.Genero = dto.Genero;
                        vExistente.Persona.EstadoCivil = dto.EstadoCivil;
                        vExistente.Persona.CiudadId = dto.CiudadId ?? 1; // Permite guardar la actualización de ciudad
                        vExistente.Persona.CodigoPostal = dto.CodigoPostal;
                        vExistente.Persona.ActualizadoEl = DateTime.Now;

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

        // DELETE: /Admin/EliminarVendedor
        [HttpDelete("EliminarVendedor")]
        public async Task<IActionResult> EliminarVendedor(int id)
        {
            try
            {
                var vendedor = await _context.Vendedores
                    .Include(v => v.Persona)
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vendedor == null) return NotFound(new { message = "El vendedor seleccionado no existe." });

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