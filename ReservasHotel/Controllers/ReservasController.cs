using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReservasHotel.DTOs;
using static ReservasHotel.AppDBContext;

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
        public async Task<ActionResult<AppDBContext.Reserva>> PostReserva(ReservaCreateDto reservaDTO)
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
        public async Task<IActionResult> PatchReserva(int ReservaId, ReservaUpdateDto reservaDTO)
        {
            if (reservaDTO == null) return BadRequest("La reserva no puede ser nula.");

            var reservaExistente = await _appDBcontext.Reservas.FindAsync(ReservaId);
            if (reservaExistente == null) return NotFound("Reserva no encontrada.");

            if (!ValidarFechas(reservaDTO, reservaExistente, out DateTime fechaInicio, out DateTime fechaFin, out string error))
                return BadRequest(error);

            if (!await ValidarYActualizarCliente(reservaDTO, reservaExistente))
                return NotFound("El cliente especificado no existe.");

            if (!await ValidarYActualizarHabitacion(reservaDTO, reservaExistente))
                return BadRequest("La habitación especificada no está disponible o no existe.");

            if (await ExisteReservaSolapada(reservaExistente, ReservaId, fechaInicio, fechaFin))
                return BadRequest("La habitación ya está reservada para las fechas especificadas.");

            ActualizarFechasReserva(reservaDTO, reservaExistente);

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

        private static bool ValidarFechas(ReservaUpdateDto reservaDTO, Reserva reservaExistente, out DateTime fechaInicio, out DateTime fechaFin, out string error)
        {
            fechaInicio = reservaDTO.FechaInicio ?? reservaExistente.FechaInicio;
            fechaFin = reservaDTO.FechaFin ?? reservaExistente.FechaFin;

            if (fechaFin <= fechaInicio)
            {
                error = "La fecha de fin debe ser mayor a la fecha de inicio.";
                return false;
            }

            if (fechaInicio < DateTime.Now || fechaFin < DateTime.Now)
            {
                error = "Las fechas de inicio y fin no pueden estar en el pasado.";
                return false;
            }

            if ((fechaFin - fechaInicio).TotalDays < 1)
            {
                error = "La reserva debe tener una duración mínima de una noche.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private async Task<bool> ValidarYActualizarCliente(ReservaUpdateDto reservaDTO, Reserva reservaExistente)
        {
            if (reservaDTO.ClienteId.HasValue)
            {
                var cliente = await _appDBcontext.Clientes.FindAsync(reservaDTO.ClienteId.Value);
                if (cliente == null) return false;
                reservaExistente.ClienteId = reservaDTO.ClienteId.Value;
            }
            return true;
        }

        private async Task<bool> ValidarYActualizarHabitacion(ReservaUpdateDto reservaDTO, Reserva reservaExistente)
        {
            if (reservaDTO.HabitacionId.HasValue)
            {
                var habitacion = await _appDBcontext.Habitaciones.FindAsync(reservaDTO.HabitacionId.Value);
                if (habitacion == null || !habitacion.Disponible) return false;
                reservaExistente.HabitacionId = reservaDTO.HabitacionId.Value;
            }
            return true;
        }

        private async Task<bool> ExisteReservaSolapada(Reserva reservaExistente, int reservaId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _appDBcontext.Reservas
                .AnyAsync(r => r.HabitacionId == reservaExistente.HabitacionId &&
                               r.ReservaId != reservaId &&
                               r.FechaInicio < fechaFin &&
                               r.FechaFin > fechaInicio);
        }

        private static void ActualizarFechasReserva(ReservaUpdateDto reservaDTO, Reserva reservaExistente)
        {
            if (reservaDTO.FechaInicio.HasValue)
                reservaExistente.FechaInicio = reservaDTO.FechaInicio.Value;

            if (reservaDTO.FechaFin.HasValue)
                reservaExistente.FechaFin = reservaDTO.FechaFin.Value;
        }

    }
}
