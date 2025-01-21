namespace ReservasHotel.DTOs
{
    public class HabitacionCreateDto
    {
        public required int NumHabitacion { get; set; }
        public required string Tipo { get; set; }
        public required decimal PrecioPorNoche { get; set; }
    }

    public class HabitacionUpdateDto
    {
        public int? NumHabitacion { get; set; }
        public string? Tipo { get; set; }
        public decimal? PrecioPorNoche { get; set; }
        public bool? Disponible { get; set; }
    }
}
