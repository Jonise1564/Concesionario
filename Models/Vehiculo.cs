using System;
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
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio")]
    [StringLength(100)]
    public string Modelo { get; set; } = string.Empty;

    [StringLength(17, ErrorMessage = "El VIN no puede superar los 17 caracteres")]
    public string? Vin { get; set; }

    [StringLength(10, ErrorMessage = "La patente no puede superar los 10 caracteres")]
    public string? Patente { get; set; }

    public string? Version { get; set; }

    [Required(ErrorMessage = "El año es obligatorio")]
    public int Anio { get; set; }

    [Required(ErrorMessage = "La condición es obligatoria")]
    [StringLength(10)]
    public string Condicion { get; set; } = "Usado"; // Valores permitidos: "0KM", "Usado"

    public int Kilometros { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Column(TypeName = "decimal(15,2)")]
    public decimal Precio { get; set; }

    public string? Combustible { get; set; }

    public string? Transmision { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    public int CategoriaId { get; set; }

    public string? Tipo { get; set; }
    
    [NotMapped]
    public string NombreCompleto => $"{Marca} {Modelo} {Version}".Trim();
    
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;

    [Required(ErrorMessage = "El estado es obligatorio")]
    [StringLength(20)]
    public string Estado { get; set; } = "Disponible"; // Valores: "Disponible", "Reservado", "Vendido", "Pausado"
}