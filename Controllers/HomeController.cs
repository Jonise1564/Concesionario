using System;
using System.Diagnostics;
using Concesionario.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Concesionario.Data;
using Microsoft.EntityFrameworkCore;

namespace Concesionario.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public IActionResult MisConsultas()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EnviarContacto(ContactoModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Contacto", model);
        }

        // 1. Guardamos la consulta en la DB de forma segura primero
        var consulta = new Consulta
        {
            Nombre = model.Nombre,
            Email = model.Email,
            Telefono = model.Telefono,
            Interes = model.Interes,
            Modelo = model.Modelo,
            Mensaje = model.Mensaje,
            Estado = "Pendiente",
            Fecha = DateTime.Now,
            UsuarioId = null
        };

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        // 2. Intentamos enviar el mail sin arriesgar la experiencia del usuario
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
            email.To.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
            email.Subject = "Nueva consulta desde la web";

            email.Body = new TextPart("plain")
            {
                Text = $"Nombre: {model.Nombre}\n"
                     + $"WhatsApp: {model.Telefono}\n"
                     + $"Email: {model.Email}\n"
                     + $"Interés: {model.Interes}\n"
                     + $"Mensaje: {model.Mensaje}",
            };

            using var smtp = new SmtpClient();
            smtp.Timeout = 5000; 

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync("roquerobertomiguellucero@gmail.com", "yxvw pnug qtdv rjvi");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            
            TempData["Mensaje"] = "Consulta enviada correctamente ✅";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = "Consulta recibida correctamente ✅ (Aviso por email en mantenimiento)";
            Console.WriteLine($"Error controlado en MailKit: {ex.Message}");
        }

        return RedirectToAction("Contacto");
    }

    // ========================================================
    // ACCIÓN: PROCESA LAS CONSULTAS DESDE EL MODAL STOCK
    // ========================================================
    [HttpPost]
    public async Task<IActionResult> EnviarConsulta(int vehiculoId, string vehiculoDetalle, string nombre, string telefono, string email, string mensaje)
    {
        // 1. Guardar la consulta del vehículo específico en la DB
        var consulta = new Consulta
        {
            Nombre = nombre,
            Email = email,
            Telefono = telefono,
            Interes = "Vehículo en Stock",
            Modelo = vehiculoDetalle, 
            Mensaje = mensaje,
            Estado = "Pendiente",
            Fecha = DateTime.Now,
            UsuarioId = null // Se mantiene alineado con tu modelo de consultas
        };

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        // 2. Notificación por Correo Electrónico
        try
        {
            var emailMsg = new MimeMessage();
            emailMsg.From.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
            emailMsg.To.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
            emailMsg.Subject = $"Consulta de Stock: {vehiculoDetalle}";

            emailMsg.Body = new TextPart("plain")
            {
                Text = $"Interés por vehículo de Stock ID: {vehiculoId}\n"
                     + $"Vehículo: {vehiculoDetalle}\n"
                     + $"Cliente: {nombre}\n"
                     + $"WhatsApp: {telefono}\n"
                     + $"Email: {email}\n"
                     + $"Mensaje: {mensaje}",
            };

            using var smtp = new SmtpClient();
            smtp.Timeout = 5000;

            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("roquerobertomiguellucero@gmail.com", "yxvw pnug qtdv rjvi");
            await smtp.SendAsync(emailMsg);
            await smtp.DisconnectAsync(true);

            TempData["Mensaje"] = "¡Tu consulta por el vehículo fue enviada! ✅";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = "Consulta recibida correctamente ✅ (Aviso por email en mantenimiento)";
            Console.WriteLine($"Error en MailKit para stock: {ex.Message}");
        }

        // Redirige al Index del controlador de Vehículos y hace el scroll automático al catálogo
        return RedirectToRoute(new { controller = "Vehiculos", action = "Index", fragment = "vehiculos" });
    }

    [HttpGet]
    public async Task<IActionResult> GetMisConsultas(string email)
    {
        var consultas = await _context.Consultas
            .Where(c => c.Email == email)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        return Json(consultas);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}