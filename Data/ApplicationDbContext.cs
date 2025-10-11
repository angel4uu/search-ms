using Microsoft.EntityFrameworkCore;
using SearchMS.Models;

namespace SearchMS.Data
{
    public class ApplicationDbContext : DbContext // Renamed from HistorialBusquedaDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<HistorialBusqueda> HistorialBusquedas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistorialBusqueda>(entity =>
            {
                entity.ToTable("historial_busqueda");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UsuarioId)
                    .IsRequired()
                    .HasMaxLength(100); 
                
                entity.Property(e => e.TextoBusqueda)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(e => e.FechaBusqueda)
                    .HasDefaultValueSql("NOW()") 
                    .ValueGeneratedOnAdd();
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}