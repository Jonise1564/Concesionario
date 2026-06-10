using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("personas")]
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DocumentoIdentidad { get; set; }

        [Required]
        public string Nombres { get; set; }

        [Required]
        public string Apellidos { get; set; }

        [Required]
        public string Email { get; set; }

        // El signo de pregunta (?) permite mapear valores NULL desde MySQL sin romper la app
        public string? Telefono { get; set; }
        public string? TelefonoAlternativo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Direccion { get; set; }
        
        //Clave foránea hacia la tabla de ciudades
        [Required]
        [Column("CiudadId")]
        public int CiudadId { get; set; }

        public string? CodigoPostal { get; set; }
        public string? Pais { get; set; }

        public DateTime CreadoEl { get; set; } = DateTime.Now;
        public DateTime? ActualizadoEl { get; set; }
        public bool Activo { get; set; } = true;

       
        [ForeignKey("CiudadId")]
        public virtual Ciudad? Ciudad { get; set; }
    }
}