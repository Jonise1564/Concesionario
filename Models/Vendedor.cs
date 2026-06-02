// using System;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace Concesionario.Models
// {
//     [Table("vendedores")]
//     public class Vendedor
//     {
//         [Key]
//         public int Id { get; set; }
        
//         public int IdPersona { get; set; }
//         public Persona Persona { get; set; }

//         public int IdUsuario { get; set; }
//         public Usuario Usuario { get; set; }

//         public DateTime FechaContratacion { get; set; }
//         public decimal PorcentajeComision { get; set; }
//         public string Observaciones { get; set; }
//     }
// }




// using System;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace Concesionario.Models
// {
//     [Table("vendedores")]
//     public class Vendedor
//     {
//         [Key]
//         [Column("Id")]
//         public int Id { get; set; }
        
//         // Mapea la propiedad de C# (IdPersona) a la columna real en MySQL (PersonaId)
//         [Column("PersonaId")] 
//         public int IdPersona { get; set; }
        
//         [ForeignKey("IdPersona")] // Apunta a la propiedad que tiene el ID
//         public Persona Persona { get; set; }

//         // Mapea la propiedad de C# (IdUsuario) a la columna real en MySQL (UsuarioId)
//         [Column("UsuarioId")] 
//         public int IdUsuario { get; set; }
        
//         [ForeignKey("IdUsuario")] // Apunta a la propiedad que tiene el ID
//         public Usuario Usuario { get; set; }

//         [Column("FechaContratacion")]
//         public DateTime FechaContratacion { get; set; }

//         [Column("PorcentajeComision")]
//         public decimal PorcentajeComision { get; set; }

//         [Column("Observaciones")]
//         public string? Observaciones { get; set; }
//     }
// }

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