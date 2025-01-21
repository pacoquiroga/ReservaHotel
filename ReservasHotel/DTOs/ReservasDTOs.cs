namespace ReservasHotel.DTOs
{
    public class ReservaCreateDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int ClienteId { get; set; }
        public int HabitacionId { get; set; }
    }

    public class ReservaUpdateDTO
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? ClienteId { get; set; }
        public int? HabitacionId { get; set; }
    }
}
