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

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        private IActionResult BuscarPorTipo(string tipo, string titulo)
        {
            var vehiculos = _context.Vehiculos
                .Where(v => v.Tipo == tipo)
                .OrderBy(v => v.Precio)
                .Take(3)
                .ToList();

            if (vehiculos.Any())
            {
                var respuesta = titulo + "\n";

                foreach (var v in vehiculos)
                {
                    respuesta += $"- {v.Marca} {v.Modelo} {v.Anio} - ${v.Precio}\n";
                }

                respuesta += "\n¿Te interesa alguno? Puedo ayudarte con más info.";

                return Ok(new { reply = respuesta });
            }

            return Ok(new { reply = $"No tenemos {tipo.ToLower()} disponibles en este momento." });
        }

        [HttpPost]
        public IActionResult Post([FromBody] ChatRequest request)
        {
            var mensaje = request.Message.ToLower();

            // 🔹 SALUDO
            if (mensaje.Contains("hola") || mensaje.Contains("buenas"))
            {
                return Ok(new { reply = "¡Hola! 👋 Bienvenido a la concesionaria Jonel. ¿Qué tipo de vehículo estás buscando?" });
            }

            // 🔹 BUSQUEDA POR MODELOS ESPECÍFICOS
            if (mensaje.Contains("hilux"))
            {
                return BuscarVehiculos("Hilux", "Tenemos estas Toyota Hilux disponibles:");
            }

            if (mensaje.Contains("gol"))
            {
                return BuscarVehiculos("Gol", "Tenemos estos Volkswagen Gol disponibles:");
            }

            if (mensaje.Contains("fiesta"))
            {
                return BuscarVehiculos("Fiesta", "Tenemos estos Ford Fiesta disponibles:");
            }

            // 🔹 BUSQUEDA POR TIPO
            if (mensaje.Contains("camioneta"))
            {
                return BuscarPorTipo("Camioneta", "Estas son algunas camionetas disponibles:");
            }

            if (mensaje.Contains("suv"))
            {
                return BuscarPorTipo("SUV", "Estas son algunas SUV disponibles:");
            }

            if (mensaje.Contains("auto"))
            {
                return BuscarPorTipo("Auto", "Estos son algunos autos disponibles:");
            }


            if (mensaje.Contains("camio"))
            {
                return BuscarPorTipo("Camioneta", "Estas son algunas camionetas disponibles:");
            }

            if (mensaje.Contains("4x4") || mensaje.Contains("alta") || mensaje.Contains("grande"))
            {
                return BuscarPorTipo("SUV", "Estas son algunas SUV disponibles:");
            }

            // 🔹 PRECIO
            if (mensaje.Contains("precio") || mensaje.Contains("cuanto"))
            {
                return Ok(new { reply = "Los precios varían según el modelo y el año. ¿Qué vehículo te interesa?" });
            }

            // 🔹 FINANCIACIÓN
            if (mensaje.Contains("financiacion") || mensaje.Contains("cuotas"))
            {
                return Ok(new { reply = "Sí, ofrecemos financiación en cuotas. ¿Querés que te asesore según tu presupuesto?" });
            }

            // 🔹 PERMUTA
            if (mensaje.Contains("permuta"))
            {
                return Ok(new { reply = "Aceptamos permutas. Podés entregar tu vehículo como parte de pago. ¿Qué auto tenés?" });
            }

            // 🔹 UBICACIÓN
            if (mensaje.Contains("donde") || mensaje.Contains("ubicacion"))
            {
                return Ok(new { reply = "Estamos en Av. España 1234, San Luis. ¿Querés que te pase la ubicación por WhatsApp?" });
            }

            // 🔹 HORARIOS
            if (mensaje.Contains("horario"))
            {
                return Ok(new { reply = "Nuestro horario es de lunes a viernes de 9 a 20 hs y sábados de 9 a 13 hs." });
            }

            // 🔹 CONTACTO
            if (mensaje.Contains("comprar") || mensaje.Contains("interesado"))
            {
                return Ok(new { reply = "¡Genial! Podés escribirnos por WhatsApp al +54 266 400-0000 y te asesoramos al instante 📲" });
            }

            // 🔹 DEFAULT
            return Ok(new { reply = "Puedo ayudarte a encontrar el vehículo ideal 🚗 ¿Buscás auto, camioneta o algún modelo específico?" });
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
    }
}