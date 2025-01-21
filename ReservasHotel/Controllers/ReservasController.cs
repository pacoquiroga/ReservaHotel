using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReservasHotel.DTOs;

namespace ReservasHotel.Controllers
{
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _appDBcontext.Reservas.ToListAsync();
            return Ok(reservas);
        }

        [HttpGet("{ReservaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppDBContext.Reserva>> GetReserva(int ReservaId)
        {
            var reserva = await _appDBcontext.Reservas.FindAsync(ReservaId);
            if (reserva == null) return NotFound("Reserva no encontrada.");
            return Ok(reserva);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppDBContext.Reserva>> PostReserva(ReservaCreateDTO reservaDTO)
        {
            if (reservaDTO == null) return BadRequest("La reserva no puede ser nula.");
            if (reservaDTO.FechaInicio == default || reservaDTO.FechaFin == default)
                return BadRequest("Las fechas de inicio y fin son obligatorias.");

            if (reservaDTO.FechaFin <= reservaDTO.FechaInicio)
                return BadRequest("La fecha de fin debe ser mayor a la fecha de inicio.");

            if (reservaDTO.FechaInicio < DateTime.Now || reservaDTO.FechaFin < DateTime.Now)
                return BadRequest("Las fechas de inicio y fin no pueden estar en el pasado.");

            if ((reservaDTO.FechaFin - reservaDTO.FechaInicio).TotalDays < 1)
                return BadRequest("La reserva debe tener una duración mínima de una noche.");

            var cliente = await _appDBcontext.Clientes.FindAsync(reservaDTO.ClienteId);
            if (cliente == null)
                return NotFound("El cliente especificado no existe.");

            var habitacion = await _appDBcontext.Habitaciones.FindAsync(reservaDTO.HabitacionId);
            if (habitacion == null)
                return NotFound("La habitación especificada no existe.");

            if (!habitacion.Disponible)
                return BadRequest("La habitación especificada no está disponible.");

            // Validación de solapamiento de reservas
            var reservasSolapadas = await _appDBcontext.Reservas
                .AnyAsync(r => r.HabitacionId == reservaDTO.HabitacionId &&
                               r.FechaInicio < reservaDTO.FechaFin &&
                               r.FechaFin > reservaDTO.FechaInicio);
            if (reservasSolapadas)
                return BadRequest("La habitación ya está reservada para las fechas especificadas.");

            var reserva = new AppDBContext.Reserva
            {
                FechaInicio = reservaDTO.FechaInicio,
                FechaFin = reservaDTO.FechaFin,
                ClienteId = reservaDTO.ClienteId,
                HabitacionId = reservaDTO.HabitacionId
            };

            _appDBcontext.Reservas.Add(reserva);
            await _appDBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReserva), new { ReservaId = reserva.ReservaId }, reserva);
        }

        [HttpPatch("{ReservaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchReserva(int ReservaId, ReservaUpdateDTO reservaDTO)
        {
            if (reservaDTO == null) return BadRequest("La reserva no puede ser nula.");

            var reservaExistente = await _appDBcontext.Reservas.FindAsync(ReservaId);
            if (reservaExistente == null) return NotFound("Reserva no encontrada.");

            // Validaciones para las fechas
            DateTime fechaInicio = reservaDTO.FechaInicio ?? reservaExistente.FechaInicio;
            DateTime fechaFin = reservaDTO.FechaFin ?? reservaExistente.FechaFin;

            if (fechaFin <= fechaInicio)
                return BadRequest("La fecha de fin debe ser mayor a la fecha de inicio.");

            if (fechaInicio < DateTime.Now || fechaFin < DateTime.Now)
                return BadRequest("Las fechas de inicio y fin no pueden estar en el pasado.");

            if ((fechaFin - fechaInicio).TotalDays < 1)
                return BadRequest("La reserva debe tener una duración mínima de una noche.");

            // Validación para la existencia del cliente
            if (reservaDTO.ClienteId.HasValue)
            {
                var cliente = await _appDBcontext.Clientes.FindAsync(reservaDTO.ClienteId.Value);
                if (cliente == null)
                    return NotFound("El cliente especificado no existe.");
                reservaExistente.ClienteId = reservaDTO.ClienteId.Value;
            }

            // Validación para la existencia de la habitación y su disponibilidad
            if (reservaDTO.HabitacionId.HasValue)
            {
                var habitacion = await _appDBcontext.Habitaciones.FindAsync(reservaDTO.HabitacionId.Value);
                if (habitacion == null)
                    return NotFound("La habitación especificada no existe.");
                if (!habitacion.Disponible)
                    return BadRequest("La habitación especificada no está disponible.");
                reservaExistente.HabitacionId = reservaDTO.HabitacionId.Value;
            }

            // Validación de solapamiento de reservas
            var reservasSolapadas = await _appDBcontext.Reservas
                .AnyAsync(r => r.HabitacionId == reservaExistente.HabitacionId &&
                               r.ReservaId != ReservaId &&
                               r.FechaInicio < fechaFin &&
                               r.FechaFin > fechaInicio);
            if (reservasSolapadas)
                return BadRequest("La habitación ya está reservada para las fechas especificadas.");

            if (reservaDTO.FechaInicio.HasValue)
                reservaExistente.FechaInicio = reservaDTO.FechaInicio.Value;
            if (reservaDTO.FechaFin.HasValue)
                reservaExistente.FechaFin = reservaDTO.FechaFin.Value;

            await _appDBcontext.SaveChangesAsync();
            return Ok(reservaExistente);
        }

        [HttpDelete("{ReservaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReserva(int ReservaId)
        {
            var reserva = await _appDBcontext.Reservas.FindAsync(ReservaId);
            if (reserva == null) return NotFound("Reserva no encontrada.");

            _appDBcontext.Reservas.Remove(reserva);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Reserva eliminada: {reserva.ReservaId}");
        }
    }
}
