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
    public class HabitacionesController : ControllerBase
    {
        private readonly AppDBContext _appDBcontext;

        public HabitacionesController(AppDBContext context)
        {
            _appDBcontext = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _appDBcontext.Habitaciones.ToListAsync();
            return Ok(habitaciones);
        }

        [HttpGet("{HabitacionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppDBContext.Habitacion>> GetHabitacion(int HabitacionId)
        {
            var habitacion = await _appDBcontext.Habitaciones.FindAsync(HabitacionId);
            if (habitacion == null) return NotFound("Habitación no encontrada.");
            return Ok(habitacion);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppDBContext.Habitacion>> PostHabitacion(HabitacionCreateDto habitacionDto)
        {
            if (habitacionDto == null) return BadRequest("La habitación no puede ser nula.");
            if (habitacionDto.PrecioPorNoche <= 0) return BadRequest("El precio por noche no puede ser menor a 0.");
            if (habitacionDto.PrecioPorNoche > 100) return BadRequest("El precio por noche no puede exceder los $100.");
            if (string.IsNullOrEmpty(habitacionDto.Tipo)) return BadRequest("El tipo de habitación es obligatorio.");
            if (habitacionDto.Tipo.Length > 50) return BadRequest("El tipo de habitación no puede exceder los 50 caracteres.");

            // Validar si el número de habitación ya existe
            var habitacionExistente = await _appDBcontext.Habitaciones
                .FirstOrDefaultAsync(h => h.NumHabitacion == habitacionDto.NumHabitacion);
            if (habitacionExistente != null) return BadRequest("El número de habitación ya existe.");

            var habitacion = new AppDBContext.Habitacion
            {
                NumHabitacion = habitacionDto.NumHabitacion,
                Tipo = habitacionDto.Tipo,
                PrecioPorNoche = habitacionDto.PrecioPorNoche,
                Disponible = true,
            };

            _appDBcontext.Habitaciones.Add(habitacion);
            await _appDBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHabitacion), new { HabitacionId = habitacion.HabitacionId }, habitacion);
        }

        [HttpPatch("{HabitacionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchHabitacion(int HabitacionId, HabitacionUpdateDto habitacionDTO)
        {
            if (habitacionDTO == null) return BadRequest("La habitación no puede ser nula.");

            var habitacionExistente = await _appDBcontext.Habitaciones.FindAsync(HabitacionId);
            if (habitacionExistente == null) return NotFound("Habitación no encontrada.");

            if (!await ValidarYActualizarNumeroHabitacion(HabitacionId, habitacionDTO, habitacionExistente))
                return BadRequest("El número de habitación ya existe.");

            if (!ValidarYActualizarPrecio(habitacionDTO, habitacionExistente, out var precioError))
                return BadRequest(precioError);

            if (!ValidarYActualizarTipo(habitacionDTO, habitacionExistente, out var tipoError))
                return BadRequest(tipoError);

            if (!await ValidarYActualizarDisponibilidad(HabitacionId, habitacionDTO, habitacionExistente))
                return BadRequest("La habitación no puede ser marcada como no disponible porque tiene reservas activas.");

            await _appDBcontext.SaveChangesAsync();
            return Ok(habitacionExistente);
        }

        [HttpDelete("{HabitacionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteHabitacion(int HabitacionId)
        {
            var habitacion = await _appDBcontext.Habitaciones.FindAsync(HabitacionId);
            if (habitacion == null) return NotFound("Habitación no encontrada.");

            // Validar que la habitación no tenga reservas activas
            var reservasActivas = await _appDBcontext.Reservas
                .AnyAsync(r => r.HabitacionId == HabitacionId && r.FechaFin > DateTime.Now);
            if (reservasActivas)
                return BadRequest("La habitación no puede ser eliminada porque tiene reservas activas.");

            _appDBcontext.Habitaciones.Remove(habitacion);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Habitación eliminada: {habitacion.HabitacionId}");
        }

        private async Task<bool> ValidarYActualizarNumeroHabitacion(int habitacionId, HabitacionUpdateDto habitacionDTO, Habitacion habitacionExistente)
        {
            if (habitacionDTO.NumHabitacion.HasValue)
            {
                var habitacionConMismoNumero = await _appDBcontext.Habitaciones
                    .FirstOrDefaultAsync(h => h.NumHabitacion == habitacionDTO.NumHabitacion && h.HabitacionId != habitacionId);

                if (habitacionConMismoNumero != null)
                    return false;

                habitacionExistente.NumHabitacion = habitacionDTO.NumHabitacion.Value;
            }
            return true;
        }

        private bool ValidarYActualizarPrecio(HabitacionUpdateDto habitacionDTO, Habitacion habitacionExistente, out string error)
        {
            error = string.Empty;

            if (habitacionDTO.PrecioPorNoche.HasValue)
            {
                if (habitacionDTO.PrecioPorNoche <= 0)
                {
                    error = "El precio por noche no puede ser menor a 0.";
                    return false;
                }

                if (habitacionDTO.PrecioPorNoche > 100)
                {
                    error = "El precio por noche no puede exceder los $100.";
                    return false;
                }

                habitacionExistente.PrecioPorNoche = habitacionDTO.PrecioPorNoche.Value;
            }

            return true;
        }

        private bool ValidarYActualizarTipo(HabitacionUpdateDto habitacionDTO, Habitacion habitacionExistente, out string error)
        {
            error = string.Empty;

            if (!string.IsNullOrEmpty(habitacionDTO.Tipo))
            {
                if (habitacionDTO.Tipo.Length > 50)
                {
                    error = "El tipo de habitación no puede exceder los 50 caracteres.";
                    return false;
                }

                habitacionExistente.Tipo = habitacionDTO.Tipo;
            }

            return true;
        }

        private async Task<bool> ValidarYActualizarDisponibilidad(int habitacionId, HabitacionUpdateDto habitacionDTO, Habitacion habitacionExistente)
        {
            if (habitacionDTO.Disponible.HasValue)
            {
                if (!habitacionDTO.Disponible.Value)
                {
                    var reservasActivas = await _appDBcontext.Reservas
                        .AnyAsync(r => r.HabitacionId == habitacionId && r.FechaFin > DateTime.Now);

                    if (reservasActivas)
                        return false;
                }

                habitacionExistente.Disponible = habitacionDTO.Disponible.Value;
            }

            return true;
        }

    }
}
