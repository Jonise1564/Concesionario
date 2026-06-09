using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Concesionario.Models
{
    [Table("ciudades")]
    public class Ciudad
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("ProvinciaId")]
        public int ProvinciaId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        // Propiedades de navegación (Relación N:1 con Provincias)
        [ForeignKey("ProvinciaId")]
        [JsonIgnore] // Evita ciclos infinitos si serializás a JSON directamente
        public virtual Provincia? Provincia { get; set; }
    }
}