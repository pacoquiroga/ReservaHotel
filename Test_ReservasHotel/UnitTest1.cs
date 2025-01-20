using Xunit;
using static ReservasHotel.AppDBContext;

namespace Test_ReservasHotel
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var habitacion = new Habitacion { tipo = "Prueba", PrecioPorNoche=20, Disponible=false };

            Assert.True(habitacion.PrecioPorNoche >= 0, "El precio no puede ser negativo");
        }
    }
}