

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concesionario.Models
{
    [Table("vendedores")]
    public class Vendedor
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        
        [Column("PersonaId")] 
        public int IdPersona { get; set; }
        
        [ForeignKey("IdPersona")]
        public Persona Persona { get; set; }

        [Column("UsuarioId")] 
        public int IdUsuario { get; set; }
        
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

        [Column("FechaContratacion")]
        public DateTime FechaContratacion { get; set; }

        [Column("PorcentajeComision")]
        public decimal PorcentajeComision { get; set; }

        // El signo de pregunta '?' evita el SqlNullValueException si la columna en MySQL es NULL
        [Column("Observaciones")]
        public string? Observaciones { get; set; }
    }
}