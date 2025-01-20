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

        public class Cliente
        {
            public int? id { get; set; } 

            public required string nombre { get; set; }

            public required string apellido { get; set; }

            public string? telefono { get; set; }

            
            public ICollection<Reserva>? reservas { get; set; }
        }

        public class Habitacion
        {
            public int? id { get; set; } 
            public int? numHabitacion { get; set; }

            public required string tipo { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            public required decimal PrecioPorNoche { get; set; }

            public required bool Disponible { get; set; } 


            public ICollection<Reserva>? reservas { get; set; }
        }

        public class Reserva
        {
            public int? id { get; set; } 

            public required DateTime fechaInicio { get; set; }

            public required DateTime fechaFin { get; set; }

            public required int clienteId { get; set; } 

            public required int habitacionId { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            public required decimal precioTotal { get; set; } 

            public ICollection<ServicioAdicional>? serviciosAdicionales { get; set; }
        }

        
        public class ServicioAdicional
        {
            public int? id { get; set; } 

            public required string descripcion { get; set; }

            [Column(TypeName = "decimal(10,2)")]
            public required decimal costo { get; set; }

            public required int idReserva { get; set; } 
        }


    }
}
