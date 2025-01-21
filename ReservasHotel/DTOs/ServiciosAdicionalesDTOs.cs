namespace ReservasHotel.DTOs
{
    public class ServicioAdicionalCreateDTO
    {
        public required string Descripcion { get; set; }
        public decimal Costo { get; set; }
        public int ReservaId { get; set; }
    }

    public class ServicioAdicionalUpdateDTO
    {
        public string? Descripcion { get; set; }
        public decimal? Costo { get; set; }
        public int? ReservaId { get; set; }
    }
}
