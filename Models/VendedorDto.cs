using System;

namespace Concesionario.Models
{
    public class VendedorDto
    {
        public int Id { get; set; }
        public string DocumentoIdentidad { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string NombreUsuario { get; set; }
        public string Password { get; set; } 
        public decimal PorcentajeComision { get; set; }
        public string Observaciones { get; set; }
    }
}