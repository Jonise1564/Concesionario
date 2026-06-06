using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Concesionario.Data; 
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

    // LISTADO PÚBLICO: Muestra el catálogo de autos en stock para los clientes
    public async Task<IActionResult> Index()
    {
        // Filtramos para traer solo los vehículos aptos para la venta:
        // 1. Que estén comerciales como 'Disponible'
        // 2. Que la bandera técnica 'Activo' sea verdadera (no borrado/oculto)
        var inventario = await _context.Vehiculos
            .Where(v => v.Estado == "Disponible" && v.Activo == true)
            .ToListAsync();

        return View(inventario);
    }

    public IActionResult Buscar()
    {
        return View();
    }
}