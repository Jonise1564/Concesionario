using System.Diagnostics;
using Concesionario.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

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

    [HttpPost]
    public async Task<IActionResult> EnviarContacto(ContactoModel model)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
        email.To.Add(MailboxAddress.Parse("roquerobertomiguellucero@gmail.com"));
        email.Subject = "Nueva consulta desde la web";

        email.Body = new TextPart("plain")
        {
            Text =
                $"Nombre: {model.Nombre}\n"
                + $"WhatsApp: {model.Telefono}\n"
                + $"Email: {model.Email}\n"
                + $"Interés: {model.Interes}\n"
                + $"Mensaje: {model.Mensaje}",
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            "smtp.gmail.com",
            587,
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync("roquerobertomiguellucero@gmail.com", "yxvw pnug qtdv rjvi");

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        TempData["Mensaje"] = "Consulta enviada correctamente ✅";

        return RedirectToAction("Contacto");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
