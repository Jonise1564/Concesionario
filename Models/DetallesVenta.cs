using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Concesionario.Models
{
    [Table("detalle_ventas")]
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }

        public int? VehiculoId { get; set; } 

        public int? RepuestoId { get; set; } 

        public int? ServicioId { get; set; }

        [Required]
        public int Cantidad { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal PrecioUnitario { get; set; }

        // ==========================================
        // PROPIEDADES DE NAVEGACIÓN (RELACIONES)
        // ==========================================
        
        [JsonIgnore] // Sella el bucle de recursión infinita al serializar JSON
        [ForeignKey("VentaId")]
        public virtual Venta? Venta { get; set; }

        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }
        
        // Si más adelante creámos los modelos de Repuestos o Servicios:
        // [ForeignKey("RepuestoId")]
        // public virtual Repuesto? Repuesto { get; set; }
        
        // [ForeignKey("ServicioId")]
        // public virtual Servicio? Servicio { get; set; }
    }
}