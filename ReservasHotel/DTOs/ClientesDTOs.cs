namespace ReservasHotel.DTOs
{
    public class ClienteCreateDto
    {
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }

        public required int Edad { get; set; }
    }

    public class ClienteUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public int? Edad { get; set; }
    }
}
