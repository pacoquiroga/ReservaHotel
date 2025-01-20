using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ReservasHotel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDBContext _appDBcontext;

        public ClientesController(AppDBContext appDBcontext)
        {
            _appDBcontext = appDBcontext;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _appDBcontext.Clientes.ToListAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppDBContext.Cliente>> GetCliente(int id)
        {
            var cliente = await _appDBcontext.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            return cliente;
        }

        [HttpPost]
        public async Task<ActionResult<AppDBContext.Cliente>> PostCliente(AppDBContext.Cliente cliente)
        {
            _appDBcontext.Clientes.Add(cliente);
            await _appDBcontext.SaveChangesAsync();
            return Ok(cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, AppDBContext.Cliente cliente)
        {

            var clienteExistente = await _appDBcontext.Clientes.FindAsync(id);
            if (clienteExistente == null) return NotFound("Cliente no encontrado");

            clienteExistente.nombre = cliente.nombre;
            clienteExistente.apellido = cliente.apellido;
            clienteExistente.telefono = cliente.telefono;
            

            await _appDBcontext.SaveChangesAsync();
            return Ok(clienteExistente);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _appDBcontext.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            _appDBcontext.Clientes.Remove(cliente);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Cliente eliminado: {cliente.nombre + cliente.apellido}"); 
        }
    }



    [ApiController]
    [Route("api/[controller]")]
    public class HabitacionesController : ControllerBase
    {
        private readonly AppDBContext _appDBcontext;

        public HabitacionesController(AppDBContext context)
        {
            _appDBcontext = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _appDBcontext.Habitaciones.ToListAsync();
            return Ok(habitaciones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppDBContext.Habitacion>> GetHbitacion(int id)
        {
            var habitacion = await _appDBcontext.Habitaciones.FindAsync(id);
            if (habitacion == null) return NotFound();
            return habitacion;
        }

        [HttpPost]
        public async Task<ActionResult<AppDBContext.Habitacion>> PostHabitacion(AppDBContext.Habitacion habitacion)
        {
            if(habitacion == null) return BadRequest();
            if (habitacion.PrecioPorNoche < 0) return BadRequest("El precio por noche no puede ser negativo");
            if (habitacion.tipo == null) return BadRequest("El tipo de habitacion no puede ser nulo");
            habitacion.Disponible = true;

            _appDBcontext.Habitaciones.Add(habitacion);
            await _appDBcontext.SaveChangesAsync();
            return Ok(habitacion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutHabitacion(int id, AppDBContext.Habitacion habitacion)
        {

            if (habitacion == null) return BadRequest();
            if (habitacion.PrecioPorNoche < 0) return BadRequest("El precio por noche no puede ser negativo");
            if (habitacion.tipo == null) return BadRequest("El tipo de habitacion no puede ser nulo");

            var habitacionExistente = await _appDBcontext.Habitaciones.FindAsync(id);
            if (habitacionExistente == null) return NotFound("Habitacion no encontrado");

            habitacionExistente.tipo = habitacion.tipo;
            habitacionExistente.PrecioPorNoche = habitacion.PrecioPorNoche;
            habitacionExistente.Disponible = habitacion.Disponible;


            await _appDBcontext.SaveChangesAsync();
            return Ok(habitacionExistente);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabitacion(int id)
        {
            var habitacion = await _appDBcontext.Habitaciones.FindAsync(id);
            if (habitacion == null) return NotFound();

            _appDBcontext.Habitaciones.Remove(habitacion);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Habitacion eliminado: {habitacion.id}");
        }

    }

    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDBContext _appDBcontext;

        public ReservasController(AppDBContext context)
        {
            _appDBcontext = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _appDBcontext.Reservas.ToListAsync();
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppDBContext.Reserva>> GetReserva(int id)
        {
            var reserva = await _appDBcontext.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();
            return reserva;
        }

        [HttpPost]
        public async Task<ActionResult<AppDBContext.Reserva>> PostReserva(AppDBContext.Reserva reserva)
        {
            if (reserva.fechaInicio == default || reserva.fechaFin == default)
                return BadRequest("Las fechas de inicio y fin son obligatorias.");

            if (reserva.fechaFin <= reserva.fechaInicio)
                return BadRequest("La fecha de fin debe ser mayor a la fecha de inicio.");

            var cliente = await _appDBcontext.Clientes.FindAsync(reserva.clienteId);
            if (cliente == null)
                return NotFound("El reserva especificado no existe.");

            var habitacion = await _appDBcontext.Habitaciones.FindAsync(reserva.habitacionId);
            if (habitacion == null)
                return NotFound("La reserva especificada no existe.");

            if (!habitacion.Disponible)
                return BadRequest("La reserva especificada no está disponible.");

            
            _appDBcontext.Reservas.Add(reserva);
            habitacion.Disponible = false;
            await _appDBcontext.SaveChangesAsync();
            return Ok(reserva);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, AppDBContext.Reserva reserva)
        {

            var reservaExistente = await _appDBcontext.Reservas.FindAsync(id);
            if (reservaExistente == null) return NotFound("Reserva no encontrado");

            if (reserva.fechaInicio == default || reserva.fechaFin == default)
                return BadRequest("Las fechas de inicio y fin son obligatorias.");

            if (reserva.fechaFin <= reserva.fechaInicio)
                return BadRequest("La fecha de fin debe ser mayor a la fecha de inicio.");

            if (reserva.precioTotal < 0)
                return BadRequest("El precio total no puede ser negativo.");

            var cliente = await _appDBcontext.Clientes.FindAsync(reserva.clienteId);
            if (cliente == null)
                return NotFound("El reserva especificado no existe.");

            var habitacion = await _appDBcontext.Habitaciones.FindAsync(reserva.habitacionId);
            if (habitacion == null)
                return NotFound("La reserva especificada no existe.");
            

            reservaExistente.fechaFin = reserva.fechaFin;
            reservaExistente.fechaInicio = reserva.fechaInicio;
            reservaExistente.clienteId = reserva.clienteId;
            reservaExistente.habitacionId = reserva.habitacionId;
            reservaExistente.precioTotal = reserva.precioTotal;


            await _appDBcontext.SaveChangesAsync();
            return Ok(reservaExistente);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _appDBcontext.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            _appDBcontext.Reservas.Remove(reserva);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Reserva eliminada: {reserva.id}");
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ServiciosAdicionalesController : ControllerBase
    {
        private readonly AppDBContext _appDBcontext;

        public ServiciosAdicionalesController(AppDBContext context)
        {
            _appDBcontext = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetServicios()
        {
            var servicios = await _appDBcontext.ServiciosAdicionales.ToListAsync();
            return Ok(servicios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppDBContext.ServicioAdicional>> GetServicio(int id)
        {
            var servicio = await _appDBcontext.ServiciosAdicionales.FindAsync(id);
            if (servicio == null) return NotFound();
            return servicio;
        }

        [HttpPost]
        public async Task<ActionResult<AppDBContext.ServicioAdicional>> PostServicio(AppDBContext.ServicioAdicional servicio)
        {
            var reserva = await _appDBcontext.Reservas.FindAsync(servicio.idReserva);
            if (reserva == null)
                return NotFound("La reserva especificada no existe.");

            _appDBcontext.ServiciosAdicionales.Add(servicio);
            await _appDBcontext.SaveChangesAsync();
            return Ok(servicio);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicio(int id, AppDBContext.ServicioAdicional servicio)
        {

            var servicioExistente = await _appDBcontext.ServiciosAdicionales.FindAsync(id);
            if (servicioExistente == null) return NotFound("Servicio no encontrado");

            var reserva = await _appDBcontext.Reservas.FindAsync(servicio.idReserva);
            if (reserva == null)
                return NotFound("La reserva especificada no existe.");

            servicioExistente.descripcion = servicio.descripcion;
            servicioExistente.costo = servicio.costo;
            servicioExistente.idReserva = servicio.idReserva;


            await _appDBcontext.SaveChangesAsync();
            return Ok(servicioExistente);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicio(int id)
        {
            var servicio = await _appDBcontext.ServiciosAdicionales.FindAsync(id);
            if (servicio == null) return NotFound();

            _appDBcontext.ServiciosAdicionales.Remove(servicio);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Servicio eliminado: {servicio.id}");
        }

    }
}
