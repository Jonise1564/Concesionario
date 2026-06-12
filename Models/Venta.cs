using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("ventas")] // En minúscula para coincidir exactamente con MySQL
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VendedorId { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")] // Ajustado a la precisión (15,2) de tu tabla
        public decimal MontoFinal { get; set; }

        [Required]
        [StringLength(50)]
        public string FormaPago { get; set; } = string.Empty;

        public string? Observaciones { get; set; } // En MySQL es de tipo 'text', mapea a string directo


        // ==========================================
        // PROPIEDADES DE NAVEGACIÓN (Entity Framework)
        // ==========================================
        
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("VendedorId")]
        public virtual Vendedor? Vendedor { get; set; }


        // ==========================================
        // PROPIEDADES EXTRA PARA EL FRONTEND (JSON)
        // ==========================================
        [NotMapped]
        public string? NombreCliente { get; set; }

        [NotMapped]
        public string? DetalleVehiculo { get; set; }
    }
}