using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        // Clave foránea que apunta a la tabla 'personas'
        [Required]
        [Column("IdPersonaId")]
        public int IdPersonaId { get; set; }

        [Required]
        [Column("FechaAlta")] // <-- CORREGIDO: Apuesta al nombre físico real en la tabla de MySQL
        public DateTime IdFechaAlta { get; set; } = DateTime.Now;

        [Column("CalificacionCrediticia")]
        [StringLength(50)]
        public string? CalificacionCrediticia { get; set; }

        [Column("Observaciones")]
        public string? Observaciones { get; set; }

        // =========================================================================
        // PROPIEDAD DE NAVEGACIÓN (Relación entre Clientes y Personas)
        // =========================================================================
        // El atributo ForeignKey mapea la columna física 'IdPersonaId' con este objeto.
        [ForeignKey("IdPersonaId")]
        public virtual Persona Persona { get; set; } = null!;
    }
}