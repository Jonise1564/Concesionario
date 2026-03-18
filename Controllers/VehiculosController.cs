using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para ToListAsync
using Concesionario.Data; // Donde está tu ApplicationDbContext
using Concesionario.Models;

namespace Concesionario.Controllers;

public class VehiculosController : Controller
{
    private readonly ApplicationDbContext _context;

    // El constructor recibe el contexto de la base de datos configurado en Program.cs
    public VehiculosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Usamos Task para que la consulta sea asíncrona
    public async Task<IActionResult> Index()
    {
        // Consultamos la tabla 'Vehiculos' de MySQL
        // .Include(v => v.Categoria) es opcional si quieres traer el nombre de la categoría
        var inventario = await _context.Vehiculos.ToListAsync();

        return View(inventario);
    }
}