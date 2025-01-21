namespace ReservasHotel.DTOs
{
    public class ServicioAdicionalCreateDto
    {
        public required string Descripcion { get; set; }
        public required decimal Costo { get; set; }
        public required int ReservaId { get; set; }
    }

    public class ServicioAdicionalUpdateDto
    {
        public string? Descripcion { get; set; }
        public decimal? Costo { get; set; }
        public int? ReservaId { get; set; }
    }
}
