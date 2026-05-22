// public class Usuario
// {
//     public int Id { get; set; }
//     public string NombreUsuario { get; set; }
//     public string Password { get; set; }
//     public string Rol { get; set; } 
// }

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(30)]
    public string Rol { get; set; } = "Vendedor"; // Por defecto "Vendedor" o "Admin"

    // ESTO PERMITE EL BORRADO LÓGICO (DESACTIVAR)
    public bool Activo { get; set; } = true; 
}