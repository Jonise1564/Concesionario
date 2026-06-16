using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VendedorId { get; set; }

        [Required]
        public int TipoComprobanteId { get; set; }

        [Required]
        public int FormaPagoId { get; set; }

        [Required]
        public int PuntoVenta { get; set; }

        [Required]
        public int NroComprobante { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal MontoFinal { get; set; }

        public string? Observaciones { get; set; } 

        // ==========================================
        // PROPIEDADES DE NAVEGACIÓN
        // ==========================================
        
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("FormaPagoId")]
        public virtual FormaPago? FormaPago { get; set; }

        [ForeignKey("TipoComprobanteId")]
        public virtual TipoComprobante? TipoComprobante { get; set; }

        // Relación crucial: Una venta tiene muchos detalles
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

        [NotMapped]
        public string NombreCliente { get; set; } = string.Empty;
    }
}