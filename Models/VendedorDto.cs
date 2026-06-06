using System;

namespace Concesionario.Models
{
    public class VendedorDto
    {
        public int Id { get; set; }
        public string DocumentoIdentidad { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string? Email { get; set; }
        public string Telefono { get; set; }

        // Nuevas propiedades agregadas para complementar los Datos Personales
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }

        // Cuenta de Usuario y Configuración Comercial
        public string NombreUsuario { get; set; }
        public string? Password { get; set; } // Nullable porque al editar puede venir vacío
        public decimal PorcentajeComision { get; set; }
        public string? Observaciones { get; set; }
    }
}