// using Microsoft.EntityFrameworkCore;
// using Concesionario.Models;

// namespace Concesionario.Data
// {
//     public class ApplicationDbContext : DbContext
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
//         {
//         }

//         public DbSet<Usuario> Usuarios { get; set; }
//         public DbSet<Vehiculo> Vehiculos { get; set; }
//         public DbSet<Consulta> Consultas { get; set; }
//         public DbSet<Categoria> Categorias { get; set; }
//         public DbSet<Persona> Personas { get; set; }
//         public DbSet<Vendedor> Vendedores { get; set; }
//         public DbSet<Cliente> Clientes { get; set; } // <-- Agregado para el ABM de Clientes

//         // Mapeo explícito para asegurar la traducción de LINQ
//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             // =================================================================
//             // MAPEO DE VEHÍCULOS (Jonel Autos)
//             // =================================================================
//             modelBuilder.Entity<Vehiculo>(entity =>
//             {
//                 entity.ToTable("Vehiculos"); // Nombre exacto de tu tabla en MySQL

//                 // Forzamos el mapeo de las nuevas columnas de Jonel Autos
//                 entity.Property(v => v.Estado).HasColumnName("Estado").HasMaxLength(50);
//                 entity.Property(v => v.Condicion).HasColumnName("Condicion").HasMaxLength(50);
//                 entity.Property(v => v.Patente).HasColumnName("Patente").HasMaxLength(20);
//                 entity.Property(v => v.Vin).HasColumnName("Vin").HasMaxLength(50);
//             });

//             // =================================================================
//             // MAPEO DE CLIENTES (Relación 1:1 con Personas)
//             // =================================================================
//             modelBuilder.Entity<Cliente>(entity =>
//             {
//                 entity.ToTable("clientes"); // Nombre de la tabla en MySQL

//                 // Mapeo de la Clave Primaria
//                 entity.HasKey(c => c.Id);
//                 entity.Property(c => c.Id).HasColumnName("Id");

//                 // 🌟 CORRECCIÓN 1: Mapeamos la propiedad C# con la columna real de tu base de datos
//                 entity.Property(c => c.IdPersonaId).HasColumnName("PersonaId");

//                 // 🌟 CORRECCIÓN 2: Mapeamos la propiedad de la fecha con la columna real
//                 entity.Property(c => c.IdFechaAlta).HasColumnName("FechaAlta");

//                 entity.Property(c => c.CalificacionCrediticia).HasColumnName("CalificacionCrediticia").HasMaxLength(50);
//                 entity.Property(c => c.Observaciones).HasColumnName("Observaciones");

//                 // 🌟 CORRECCIÓN 3: Tu base de datos dice que 'PersonaId' es UNIQUE (Relación 1 a 1)
//                 // Usamos HasOne -> WithOne para que EF traduzca la consulta de forma óptima
//                 entity.HasOne(c => c.Persona)
//                       .WithOne() // Una Persona es UN solo Cliente
//                       .HasForeignKey<Cliente>(c => c.IdPersonaId) // Especificamos que la FK está en Cliente
//                       .OnDelete(DeleteBehavior.Restrict);
//             });
//         }
//     }
// }


using Microsoft.EntityFrameworkCore;
using Concesionario.Models;

namespace Concesionario.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        
        //DbSets para la normalización geográfica de Argentina
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // MAPEO DE VEHÍCULOS (Jonel Autos)
            // =================================================================
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos");

                entity.Property(v => v.Estado).HasColumnName("Estado").HasMaxLength(50);
                entity.Property(v => v.Condicion).HasColumnName("Condicion").HasMaxLength(50);
                entity.Property(v => v.Patente).HasColumnName("Patente").HasMaxLength(20);
                entity.Property(v => v.Vin).HasColumnName("Vin").HasMaxLength(50);
            });

            // =================================================================
            // MAPEO DE CLIENTES (Relación 1:1 con Personas)
            // =================================================================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");

                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.IdPersonaId).HasColumnName("PersonaId");
                entity.Property(c => c.IdFechaAlta).HasColumnName("FechaAlta");
                entity.Property(c => c.CalificacionCrediticia).HasColumnName("CalificacionCrediticia").HasMaxLength(50);
                entity.Property(c => c.Observaciones).HasColumnName("Observaciones");

                entity.HasOne(c => c.Persona)
                      .WithOne()
                      .HasForeignKey<Cliente>(c => c.IdPersonaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // =================================================================
            // MAPEO DE PROVINCIAS
            // =================================================================
            modelBuilder.Entity<Provincia>(entity =>
            {
                entity.ToTable("provincias");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnName("Id");
                entity.Property(p => p.Nombre).HasColumnName("Nombre").HasMaxLength(100).IsRequired();
                
                // Garantiza el índice UNIQUE configurado en MySQL
                entity.HasIndex(p => p.Nombre).IsUnique();
            });

            // =================================================================
            // MAPEO DE CIUDADES
            // =================================================================
            modelBuilder.Entity<Ciudad>(entity =>
            {
                entity.ToTable("ciudades");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.ProvinciaId).HasColumnName("ProvinciaId").IsRequired();
                entity.Property(c => c.Nombre).HasColumnName("Nombre").HasMaxLength(100).IsRequired();

                // Índice compuesto UNIQUE para evitar duplicados en la misma provincia
                entity.HasIndex(c => new { c.ProvinciaId, c.Nombre }).IsUnique();

                // Configuración explícita de la relación N:1 con Provincias
                entity.HasOne(c => c.Provincia)
                      .WithMany(p => p.Ciudades)
                      .HasForeignKey(c => c.ProvinciaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



            // =================================================================
// MAPEO DE PERSONAS (Jonel Autos)
// =================================================================
modelBuilder.Entity<Persona>(entity =>
{
    entity.ToTable("Personas"); // Aseguramos consistencia de mayúsculas/minúsculas
    entity.HasKey(p => p.Id);

    // Mapeo explícito de tipos y restricciones
    entity.Property(p => p.DocumentoIdentidad).HasColumnName("DocumentoIdentidad").HasMaxLength(50).IsRequired();
    entity.Property(p => p.Nombres).HasColumnName("Nombres").HasMaxLength(100).IsRequired();
    entity.Property(p => p.Apellidos).HasColumnName("Apellidos").HasMaxLength(100).IsRequired();
    entity.Property(p => p.Email).HasColumnName("Email").HasMaxLength(150).IsRequired();
    
    // Columnas opcionales (mapeadas correctamente gracias a los nulables de tu modelo)
    entity.Property(p => p.Telefono).HasColumnName("Telefono").HasMaxLength(50);
    entity.Property(p => p.TelefonoAlternativo).HasColumnName("TelefonoAlternativo").HasMaxLength(50);
    entity.Property(p => p.Genero).HasColumnName("Genero").HasMaxLength(20);
    entity.Property(p => p.EstadoCivil).HasColumnName("EstadoCivil").HasMaxLength(50);
    entity.Property(p => p.Direccion).HasColumnName("Direccion").HasMaxLength(255);
    entity.Property(p => p.CodigoPostal).HasColumnName("CodigoPostal").HasMaxLength(20);
    entity.Property(p => p.Pais).HasColumnName("Pais").HasMaxLength(50);

    // Clave Foránea Requerida hacia Ciudades
    entity.Property(p => p.CiudadId).HasColumnName("CiudadId").IsRequired();

    // Configuración de la relación N:1 (Muchas personas -> Una ciudad)
    entity.HasOne(p => p.Ciudad)
          .WithMany() // No agregamos colección de Personas en el modelo Ciudad para mantenerlo limpio
          .HasForeignKey(p => p.CiudadId)
          .OnDelete(DeleteBehavior.Restrict); // Evita borrar ciudades que tengan personas asociadas
});
        }
    }
}