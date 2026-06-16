using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("servicios")]
    public class Servicio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Costo { get; set; } // O PrecioServicio

        [Required]
        public int TiempoEstimadoMinutos { get; set; }

        public bool Activo { get; set; } = true;
    }
}