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
        
        // 🛒 Agregados y normalizados según la estructura MySQL de Jonel Autos
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<FormaPago> FormasPago { get; set; }
        public DbSet<TipoComprobante> TiposComprobante { get; set; }
        
        // DbSets para la normalización geográfica de Argentina
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
            // MAPEO DE FORMAS DE PAGO
            // =================================================================
            modelBuilder.Entity<FormaPago>(entity =>
            {
                entity.ToTable("formas_pago");
                entity.HasKey(fp => fp.Id);
                entity.Property(fp => fp.Id).HasColumnName("Id");
                entity.Property(fp => fp.Nombre).HasColumnName("Nombre").HasMaxLength(50).IsRequired();
            });

            // =================================================================
            // MAPEO DE TIPOS DE COMPROBANTE
            // =================================================================
            modelBuilder.Entity<TipoComprobante>(entity =>
            {
                entity.ToTable("tipos_comprobante");
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.Id).HasColumnName("Id");
                entity.Property(tc => tc.Nombre).HasColumnName("Nombre").HasMaxLength(50).IsRequired();
            });

            // =================================================================
            // MAPEO DE VENTAS (Ajustado a maestro-detalle)
            // =================================================================
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas"); 

                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).HasColumnName("Id");
                
                entity.Property(v => v.ClienteId).HasColumnName("ClienteId").IsRequired();
                entity.Property(v => v.VendedorId).HasColumnName("VendedorId").IsRequired();
                entity.Property(v => v.TipoComprobanteId).HasColumnName("TipoComprobanteId").IsRequired();
                entity.Property(v => v.FormaPagoId).HasColumnName("FormaPagoId").IsRequired();
                entity.Property(v => v.PuntoVenta).HasColumnName("PuntoVenta").IsRequired();
                entity.Property(v => v.NroComprobante).HasColumnName("NroComprobante").IsRequired();
                
                entity.Property(v => v.FechaVenta)
                      .HasColumnName("FechaVenta")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .IsRequired();

                entity.Property(v => v.MontoFinal)
                      .HasColumnName("MontoFinal")
                      .HasColumnType("decimal(15,2)") 
                      .IsRequired();

                entity.Property(v => v.Observaciones)
                      .HasColumnName("Observaciones")
                      .HasColumnType("text");

                // Relaciones externas de la venta
                entity.HasOne(v => v.Cliente)
                      .WithMany()
                      .HasForeignKey(v => v.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.FormaPago)
                      .WithMany()
                      .HasForeignKey(v => v.FormaPagoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.TipoComprobante)
                      .WithMany()
                      .HasForeignKey(v => v.TipoComprobanteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación 1:N hacia los detalles
                entity.HasMany(v => v.DetallesVenta)
                      .WithOne(d => d.Venta)
                      .HasForeignKey(d => d.VentaId)
                      .OnDelete(DeleteBehavior.Cascade); // Si borrás una venta, elimina sus detalles
            });

            // =================================================================
            // MAPEO DE DETALLE VENTAS
            // =================================================================
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("detalle_ventas");

                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).HasColumnName("Id");
                entity.Property(d => d.VentaId).HasColumnName("VentaId").IsRequired();
                entity.Property(d => d.VehiculoId).HasColumnName("VehiculoId"); // Permite Null (YES)
                entity.Property(d => d.RepuestoId).HasColumnName("RepuestoId"); // Permite Null (YES)
                entity.Property(d => d.ServicioId).HasColumnName("ServicioId"); // Permite Null (YES)
                
                entity.Property(d => d.Cantidad).HasColumnName("Cantidad").HasDefaultValue(1).IsRequired();
                
                entity.Property(d => d.PrecioUnitario)
                      .HasColumnName("PrecioUnitario")
                      .HasColumnType("decimal(15,2)")
                      .IsRequired();

                // Relación condicional opcional con vehículos
                entity.HasOne(d => d.Vehiculo)
                      .WithMany()
                      .HasForeignKey(d => d.VehiculoId)
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

                entity.HasIndex(c => new { c.ProvinciaId, c.Nombre }).IsUnique();

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
                entity.ToTable("Personas");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.DocumentoIdentidad).HasColumnName("DocumentoIdentidad").HasMaxLength(50).IsRequired();
                entity.Property(p => p.Nombres).HasColumnName("Nombres").HasMaxLength(100).IsRequired();
                entity.Property(p => p.Apellidos).HasColumnName("Apellidos").HasMaxLength(100).IsRequired();
                entity.Property(p => p.Email).HasColumnName("Email").HasMaxLength(150).IsRequired();
                
                entity.Property(p => p.Telefono).HasColumnName("Telefono").HasMaxLength(50);
                entity.Property(p => p.TelefonoAlternativo).HasColumnName("TelefonoAlternativo").HasMaxLength(50);
                entity.Property(p => p.Genero).HasColumnName("Genero").HasMaxLength(20);
                entity.Property(p => p.EstadoCivil).HasColumnName("EstadoCivil").HasMaxLength(50);
                entity.Property(p => p.Direccion).HasColumnName("Direccion").HasMaxLength(255);
                entity.Property(p => p.CodigoPostal).HasColumnName("CodigoPostal").HasMaxLength(20);
                entity.Property(p => p.Pais).HasColumnName("Pais").HasMaxLength(50);

                entity.Property(p => p.CiudadId).HasColumnName("CiudadId").IsRequired();

                entity.HasOne(p => p.Ciudad)
                      .WithMany() 
                      .HasForeignKey(p => p.CiudadId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}