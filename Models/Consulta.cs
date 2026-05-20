using System.ComponentModel.DataAnnotations;

namespace Concesionario.Models
{
    public class Consulta
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Email { get; set; }

        public string Telefono { get; set; }

        public string Interes { get; set; }

        public string Modelo { get; set; }

        [Required]
        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";

        public string? RespuestaAdmin { get; set; }

        // RELACIÓN CON USUARIO
        public int? UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }
    }
}