using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models;

[Table("Vehiculos")]
public class Vehiculo
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria")]
    [StringLength(50)]
    public string Marca { get; set; } = string.Empty; // Agregado

    [Required(ErrorMessage = "El modelo es obligatorio")]
    [StringLength(100)]
    public string Modelo { get; set; } = string.Empty;

    public string? Version { get; set; }

    public int Anio { get; set; }

    public int Kilometros { get; set; } // Agregado

    [Column(TypeName = "decimal(15,2)")]
    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string? Combustible { get; set; }

    public string? Transmision { get; set; }

    public int CategoriaId { get; set; }

    public string Tipo { get; set; }
    
    // Propiedad calculada útil para el buscador
    [NotMapped]
    public string NombreCompleto => $"{Marca} {Modelo} {Version}";
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;
}