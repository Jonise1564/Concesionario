using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Concesionario.Data;
using Concesionario.Models;

namespace Concesionario.Controllers;

public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==========================================================
    // 1. VISTA PRINCIPAL (Renderiza el HTML de Gestión de Usuarios)
    // ==========================================================
    // Ruta: /Usuarios o /Usuarios/Index
    public IActionResult Index()
    {
        return View();
    }

    // ==========================================================
    // 2. CONSULTA (Devuelve la lista de usuarios activos en JSON para la grilla)
    // ==========================================================
    // Ruta: /Usuarios/GetUsuarios
    [HttpGet]
    public async Task<JsonResult> GetUsuarios()
    {
        try
        {
            // Traemos solo los usuarios que no han sido borrados lógicamente
            var usuarios = await _context.Usuarios
                .Where(u => u.Activo == true)
                .Select(u => new {
                    u.Id,
                    u.NombreUsuario,
                    u.Rol
                })
                .ToListAsync();

            return Json(usuarios);
        }
        catch (Exception ex)
        {
            return Json(new { error = true, mensaje = $"Error al cargar usuarios: {ex.Message}" });
        }
    }

    // ==========================================================
    // 3. ALTA Y MODIFICACIÓN (Guardar nuevo o actualizar existente)
    // ==========================================================
    // Ruta: /Usuarios/GuardarUsuario
    [HttpPost]
    public async Task<IActionResult> GuardarUsuario([FromBody] Usuario modelo)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { ok = false, mensaje = "Los datos del formulario no son válidos." });
        }

        try
        {
            // CASO: NUEVO USUARIO (Alta)
            if (modelo.Id == 0)
            {
                // Validamos que el nombre de usuario no esté repetido entre los activos
                var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == modelo.NombreUsuario && u.Activo == true);
                if (existe) 
                {
                    return Json(new { ok = false, mensaje = "El nombre de usuario ya se encuentra registrado." });
                }

                modelo.Activo = true; // Forzamos que se cree en estado activo
                _context.Usuarios.Add(modelo);
            }
            // CASO: EDITAR USUARIO (Modificación)
            else
            {
                var usuarioDb = await _context.Usuarios.FindAsync(modelo.Id);
                if (usuarioDb == null) 
                {
                    return Json(new { ok = false, mensaje = "El usuario no existe." });
                }

                // Actualizamos los valores de la tabla
                usuarioDb.NombreUsuario = modelo.NombreUsuario;
                usuarioDb.Rol = modelo.Rol;

                // Modificamos la contraseña solo si el administrador escribió una nueva en el formulario
                if (!string.IsNullOrEmpty(modelo.Password))
                {
                    usuarioDb.Password = modelo.Password;
                }

                _context.Usuarios.Update(usuarioDb);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true, mensaje = "Usuario guardado con éxito." });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = $"Error interno en el servidor: {ex.Message}" });
        }
    }

    // ==========================================================
    // 4. BAJA LÓGICA (Desactivar en lugar de hacer un DELETE físico)
    // ==========================================================
    // Ruta: /Usuarios/EliminarUsuario
    [HttpPost]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return Json(new { ok = false, mensaje = "Usuario no encontrado." });
            }

            // Aplicamos la baja lógica cambiando el flag a false
            usuario.Activo = false;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Json(new { ok = true, mensaje = "Usuario desactivado correctamente." });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = $"Error al intentar desactivar: {ex.Message}" });
        }
    }
}