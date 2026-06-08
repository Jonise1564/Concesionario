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
        public DbSet<Cliente> Clientes { get; set; } // <-- Agregado para el ABM de Clientes

        // Mapeo explícito para asegurar la traducción de LINQ
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // MAPEO DE VEHÍCULOS (Jonel Autos)
            // =================================================================
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos"); // Nombre exacto de tu tabla en MySQL

                // Forzamos el mapeo de las nuevas columnas de Jonel Autos
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
                entity.ToTable("clientes"); // Nombre de la tabla en MySQL

                // Mapeo de la Clave Primaria
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("Id");

                // 🌟 CORRECCIÓN 1: Mapeamos la propiedad C# con la columna real de tu base de datos
                entity.Property(c => c.IdPersonaId).HasColumnName("PersonaId");

                // 🌟 CORRECCIÓN 2: Mapeamos la propiedad de la fecha con la columna real
                entity.Property(c => c.IdFechaAlta).HasColumnName("FechaAlta");

                entity.Property(c => c.CalificacionCrediticia).HasColumnName("CalificacionCrediticia").HasMaxLength(50);
                entity.Property(c => c.Observaciones).HasColumnName("Observaciones");

                // 🌟 CORRECCIÓN 3: Tu base de datos dice que 'PersonaId' es UNIQUE (Relación 1 a 1)
                // Usamos HasOne -> WithOne para que EF traduzca la consulta de forma óptima
                entity.HasOne(c => c.Persona)
                      .WithOne() // Una Persona es UN solo Cliente
                      .HasForeignKey<Cliente>(c => c.IdPersonaId) // Especificamos que la FK está en Cliente
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}