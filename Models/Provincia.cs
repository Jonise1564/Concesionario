using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("provincias")]
    public class Provincia
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        // Propiedad de navegación (Relación 1:N con Ciudades)
        public virtual ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
    }
}