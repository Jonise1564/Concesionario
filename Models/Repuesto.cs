using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("repuestos")]
    public class Repuesto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de repuesto es obligatorio")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del repuesto es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;
    }
}
