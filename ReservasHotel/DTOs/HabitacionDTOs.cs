namespace ReservasHotel.DTOs
{
    public class HabitacionCreateDTO
    {
        public int NumHabitacion { get; set; }
        public required string Tipo { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public bool Disponible { get; set; }
    }

    public class HabitacionUpdateDTO
    {
        public int? NumHabitacion { get; set; }
        public string? Tipo { get; set; }
        public decimal? PrecioPorNoche { get; set; }
        public bool? Disponible { get; set; }
    }
}
