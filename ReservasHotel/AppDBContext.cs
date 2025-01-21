using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasHotel
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación Cliente-Reserva
            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Reservas)
                .WithOne()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Habitacion-Reserva
            modelBuilder.Entity<Habitacion>()
                .HasMany(h => h.Reservas)
                .WithOne()
                .HasForeignKey(r => r.HabitacionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Reserva-ServicioAdicional
            modelBuilder.Entity<Reserva>()
                .HasMany(r => r.ServiciosAdicionales)
                .WithOne()
                .HasForeignKey(sa => sa.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public class Cliente
        {
            [Key]
            public int ClienteId { get; set; }

            [Required]
            public required string Nombre { get; set; }

            [Required]
            public required string Apellido { get; set; }

            [Phone]
            public string? Telefono { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public int Edad { get; set; }

            public ICollection<Reserva>? Reservas { get; set; }
        }

        public class Habitacion
        {
            [Key]
            public int HabitacionId { get; set; }

            [Required]
            public int NumHabitacion { get; set; }

            [Required]
            public required string Tipo { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            [Required]
            public decimal PrecioPorNoche { get; set; }

            [Required]
            public bool Disponible { get; set; }

            public ICollection<Reserva>? Reservas { get; set; }
        }

        public class Reserva
        {
            [Key]
            public int ReservaId { get; set; }

            [Required]
            public DateTime FechaInicio { get; set; }

            [Required]
            public DateTime FechaFin { get; set; }

            [Required]
            public int ClienteId { get; set; }

            [Required]
            public int HabitacionId { get; set; }

            public ICollection<ServicioAdicional>? ServiciosAdicionales { get; set; }
        }

        public class ServicioAdicional
        {
            [Key]
            public int ServicioAdicionalId { get; set; }

            [Required]
            public required string Descripcion { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            [Required]
            public decimal Costo { get; set; }

            [Required]
            public int ReservaId { get; set; }
        }
    }
}
