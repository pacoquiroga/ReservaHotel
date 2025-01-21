using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReservasHotel.DTOs;

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
        public async Task<ActionResult<AppDBContext.Habitacion>> PostHabitacion(HabitacionCreateDTO habitacionDTO)
        {
            if (habitacionDTO == null) return BadRequest("La habitación no puede ser nula.");
            if (habitacionDTO.PrecioPorNoche <= 0) return BadRequest("El precio por noche no puede ser menor a 0.");
            if (habitacionDTO.PrecioPorNoche > 100) return BadRequest("El precio por noche no puede exceder los $100.");
            if (string.IsNullOrEmpty(habitacionDTO.Tipo)) return BadRequest("El tipo de habitación es obligatorio.");
            if (habitacionDTO.Tipo.Length > 50) return BadRequest("El tipo de habitación no puede exceder los 50 caracteres.");

            // Validar si el número de habitación ya existe
            var habitacionExistente = await _appDBcontext.Habitaciones
                .FirstOrDefaultAsync(h => h.NumHabitacion == habitacionDTO.NumHabitacion);
            if (habitacionExistente != null) return BadRequest("El número de habitación ya existe.");

            var habitacion = new AppDBContext.Habitacion
            {
                NumHabitacion = habitacionDTO.NumHabitacion,
                Tipo = habitacionDTO.Tipo,
                PrecioPorNoche = habitacionDTO.PrecioPorNoche,
                Disponible = habitacionDTO.Disponible
            };

            _appDBcontext.Habitaciones.Add(habitacion);
            await _appDBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHabitacion), new { HabitacionId = habitacion.HabitacionId }, habitacion);
        }

        [HttpPatch("{HabitacionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchHabitacion(int HabitacionId, HabitacionUpdateDTO habitacionDTO)
        {
            if (habitacionDTO == null) return BadRequest("La habitación no puede ser nula.");

            var habitacionExistente = await _appDBcontext.Habitaciones.FindAsync(HabitacionId);
            if (habitacionExistente == null) return NotFound("Habitación no encontrada.");

            // Validar si el nuevo número de habitación ya existe
            if (habitacionDTO.NumHabitacion.HasValue)
            {
                var habitacionConMismoNumero = await _appDBcontext.Habitaciones
                    .FirstOrDefaultAsync(h => h.NumHabitacion == habitacionDTO.NumHabitacion && h.HabitacionId != HabitacionId);
                if (habitacionConMismoNumero != null) return BadRequest("El número de habitación ya existe.");
                habitacionExistente.NumHabitacion = habitacionDTO.NumHabitacion.Value;
            }

            // Validar que precio sea positivo si existe en el DTO
            if (habitacionDTO.PrecioPorNoche.HasValue)
            {
                if (habitacionDTO.PrecioPorNoche <= 0)
                    return BadRequest("El precio por noche no puede ser menor a 0.");
                if (habitacionDTO.PrecioPorNoche > 100)
                    return BadRequest("El precio por noche no puede exceder los $100.");
                habitacionExistente.PrecioPorNoche = habitacionDTO.PrecioPorNoche.Value;
            }

            if (!string.IsNullOrEmpty(habitacionDTO.Tipo))
            {
                if (habitacionDTO.Tipo.Length > 50)
                    return BadRequest("El tipo de habitación no puede exceder los 50 caracteres.");

                habitacionExistente.Tipo = habitacionDTO.Tipo;
            }

            if (habitacionDTO.Disponible.HasValue)
            {
                // Validar que la habitación no esté reservada si se intenta marcar como no disponible
                if (!habitacionDTO.Disponible.Value)
                {
                    var reservasActivas = await _appDBcontext.Reservas
                        .AnyAsync(r => r.HabitacionId == HabitacionId && r.FechaFin > DateTime.Now);
                    if (reservasActivas)
                        return BadRequest("La habitación no puede ser marcada como no disponible porque tiene reservas activas.");
                }
                habitacionExistente.Disponible = habitacionDTO.Disponible.Value;
            }

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
    }
}
