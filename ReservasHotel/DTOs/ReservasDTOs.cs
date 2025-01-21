namespace ReservasHotel.DTOs
{
    public class ReservaCreateDto
    {
        public required DateTime FechaInicio { get; set; }
        public required DateTime FechaFin { get; set; }
        public required int ClienteId { get; set; }
        public required int HabitacionId { get; set; }
    }

    public class ReservaUpdateDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? ClienteId { get; set; }
        public int? HabitacionId { get; set; }
    }
}
