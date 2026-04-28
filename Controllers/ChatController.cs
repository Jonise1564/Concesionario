using Microsoft.AspNetCore.Mvc;
using Concesionario.Data;
using Concesionario.Models;
using System.Linq;

namespace Concesionario.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static string ultimoTipo = "Auto";

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        private IActionResult BuscarPorTipo(string tipo, string titulo, int cantidad = 3)
        {
            var vehiculos = _context.Vehiculos
                .Where(v => v.Tipo == tipo)
                .OrderBy(v => v.Precio)
                .Take(cantidad)
                .ToList();

            var respuesta = titulo + "\n";

            foreach (var v in vehiculos)
            {
                respuesta += $"- {v.Marca} {v.Modelo} {v.Anio} - ${v.Precio:N0}\n";
            }

            respuesta += "\n¿Te interesa alguno o querés ver más opciones?";

            return Ok(new { reply = respuesta });
        }

        [HttpPost]
        public IActionResult Post([FromBody] ChatRequest request)
        {
            var detector = new IntentDetector();
            var intent = detector.Detectar(request.Message);

            switch (intent)
            {
                case ChatIntent.Saludo:
                    return Ok(new { reply = "¡Hola! 👋 ¿Buscás auto, camioneta o SUV?" });

                case ChatIntent.BuscarAuto:
                    ultimoTipo = "Auto";
                    return BuscarPorTipo("Auto", "Estos son algunos autos disponibles:");

                case ChatIntent.BuscarCamioneta:
                    ultimoTipo = "Camioneta";
                    return BuscarPorTipo("Camioneta", "Estas son algunas camionetas:");

                case ChatIntent.BuscarSUV:
                    ultimoTipo = "SUV";
                    return BuscarPorTipo("SUV", "Estas son algunas SUV:");

                case ChatIntent.BuscarModelo:
                    return BuscarPorModelo(request.Message);

                case ChatIntent.Precio:
                    return Ok(new { reply = "¿Qué vehículo te interesa? Así puedo decirte el precio." });

                case ChatIntent.Barato:
                    return BuscarPorPrecio(true);

                case ChatIntent.Caro:
                    return BuscarPorPrecio(false);

                case ChatIntent.Financiamiento:
                    return Ok(new { reply = "Ofrecemos financiación. Escribinos al WhatsApp 📲 +54 266 400-0000" });

                case ChatIntent.Permuta:
                    return Ok(new { reply = "Aceptamos permutas. ¿Qué vehículo tenés?" });

                case ChatIntent.Ubicacion:
                    return Ok(new { reply = "Estamos en Av. España 1234, San Luis." });

                case ChatIntent.Horario:
                    return Ok(new { reply = "Lun a Vie 9 a 20 hs, Sáb 9 a 13 hs." });

                case ChatIntent.InteresCompra:
                    return Ok(new { reply = "¡Genial! Escribinos por WhatsApp 📲 +54 266 400-0000" });

                case ChatIntent.Rechazo:
                    return Ok(new { reply = "No hay problema 👍 ¿Querés que te muestre otras opciones o preferís otro tipo de vehículo?" });

                case ChatIntent.VerMas:
                    return BuscarPorTipo(
                        ultimoTipo,
                        $"Te muestro más {ultimoTipo.ToLower()}:",
                        10
                    );

                default:
                    return Ok(new { reply = "No entendí 😅 ¿Buscás auto, camioneta o SUV?" });
            }
        }

        // 🔧 MÉTODO AUXILIAR PARA BUSCAR VEHÍCULOS
        private IActionResult BuscarVehiculos(string modelo, string titulo)
        {
            var vehiculos = _context.Vehiculos
                .Where(v => v.Modelo.Contains(modelo))
                .OrderBy(v => v.Precio)
                .Take(3)
                .ToList();

            if (vehiculos.Any())
            {
                var respuesta = titulo + "\n";

                foreach (var v in vehiculos)
                {
                    respuesta += $"- {v.Marca} {v.Modelo} {v.Anio} - ${string.Format(new System.Globalization.CultureInfo("es-AR"), "{0:N0}", v.Precio)}\n";
                }

                respuesta += "\n¿Te interesa alguno? Puedo ayudarte con más info.";

                return Ok(new { reply = respuesta });
            }

            return Ok(new { reply = $"No tenemos {modelo} disponibles ahora, pero puedo mostrarte alternativas. ¿Te interesa algo similar?" });
        }

        private IActionResult BuscarPorPrecio(bool barato)
        {
            var query = _context.Vehiculos.AsQueryable();

            query = barato 
                ? query.OrderBy(v => v.Precio) 
                : query.OrderByDescending(v => v.Precio);

            var vehiculos = query.Take(3).ToList();

            var respuesta = barato ? "Opciones económicas:\n" : "Opciones premium:\n";

            foreach (var v in vehiculos)
            {
                respuesta += $"- {v.Marca} {v.Modelo} {v.Anio} - ${v.Precio:N0}\n";
            }

            return Ok(new { reply = respuesta });
        }

        private IActionResult BuscarPorModelo(string mensaje)
        {
            var vehiculos = _context.Vehiculos
                .Where(v => mensaje.Contains(v.Modelo.ToLower()))
                .Take(3)
                .ToList();

            if (!vehiculos.Any())
                return Ok(new { reply = "No encontré ese modelo 😅 ¿Querés ver autos, camionetas o SUV?" });

            var respuesta = "Encontré esto:\n";

            foreach (var v in vehiculos)
            {
                respuesta += $"- {v.Marca} {v.Modelo} {v.Anio} - ${v.Precio:N0}\n";
            }

            return Ok(new { reply = respuesta });
        }
    }
}