using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string Password { get; set; }

        // Si en la base de datos 'Rol' puede ser NULL en algún registro, ponelo así:
        public string? Rol { get; set; }

        public bool Activo { get; set; } = true;

        public string Email { get; set; }
    }
}