using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Concesionario.Models;

namespace Concesionario.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Financiamiento() => View();
    public IActionResult SobreNosotros() => View();
    public IActionResult Contacto() => View();
    public IActionResult Acceso() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
