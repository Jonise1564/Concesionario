// using System;

// namespace Concesionario.Models
// {
//     public class VendedorDto
//     {
//         public int Id { get; set; }
//         public string DocumentoIdentidad { get; set; }
//         public string Nombres { get; set; }
//         public string Apellidos { get; set; }
//         public string? Email { get; set; }
//         public string Telefono { get; set; }

//         // Nuevas propiedades agregadas para complementar los Datos Personales
//         public DateTime? FechaNacimiento { get; set; }
//         public string? Genero { get; set; }
//         public string? EstadoCivil { get; set; }
//         public string? Provincia { get; set; }
//         public int? CiudadId { get; set; } // 👈 Agregado para sincronizar con la entidad Persona
//         public string? CodigoPostal { get; set; }

//         // Cuenta de Usuario y Configuración Comercial
//         public string NombreUsuario { get; set; }
//         public string? Password { get; set; } // Nullable porque al editar puede venir vacío
//         public decimal PorcentajeComision { get; set; }
//         public string? Observaciones { get; set; }
//     }
// }




using System;
using System.Text.Json.Serialization; // 👈 Espectacularmente importante agregar este using

namespace Concesionario.Models
{
    public class VendedorDto
    {
        public int Id { get; set; }
        
        [JsonPropertyName("documentoIdentidad")]
        public string DocumentoIdentidad { get; set; }
        
        [JsonPropertyName("nombres")]
        public string Nombres { get; set; }
        
        [JsonPropertyName("apellidos")]
        public string Apellidos { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("telefono")]
        public string Telefono { get; set; }

        // Nuevas propiedades agregadas para complementar los Datos Personales
        [JsonPropertyName("fechaNacimiento")]
        public DateTime? FechaNacimiento { get; set; }
        
        [JsonPropertyName("genero")]
        public string? Genero { get; set; }
        
        [JsonPropertyName("estadoCivil")]
        public string? EstadoCivil { get; set; }
        
        [JsonPropertyName("provincia")]
        public string? Provincia { get; set; }
        
        [JsonPropertyName("ciudadId")]
        public int? CiudadId { get; set; } 
        
        [JsonPropertyName("codigoPostal")]
        public string? CodigoPostal { get; set; }

        // Cuenta de Usuario y Configuración Comercial
        [JsonPropertyName("nombreUsuario")] // 🚨 Esto blinda el campo contra diferencias de mayúsculas/minúsculas
        public string NombreUsuario { get; set; }
        
        [JsonPropertyName("password")]
        public string? Password { get; set; } 
        
        [JsonPropertyName("porcentajeComision")]
        public decimal PorcentajeComision { get; set; }
        
        [JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }
    }
}