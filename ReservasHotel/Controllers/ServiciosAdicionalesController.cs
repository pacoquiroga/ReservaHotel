using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReservasHotel.DTOs;

namespace ReservasHotel.Controllers
{
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServicios()
        {
            var servicios = await _appDBcontext.ServiciosAdicionales.ToListAsync();
            return Ok(servicios);
        }

        [HttpGet("{ServicioAdicionalId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppDBContext.ServicioAdicional>> GetServicio(int ServicioAdicionalId)
        {
            var servicio = await _appDBcontext.ServiciosAdicionales.FindAsync(ServicioAdicionalId);
            if (servicio == null) return NotFound("Servicio adicional no encontrado.");
            return Ok(servicio);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppDBContext.ServicioAdicional>> PostServicio(ServicioAdicionalCreateDTO servicioDTO)
        {
            if (servicioDTO == null) return BadRequest("El servicio adicional no puede ser nulo.");
            if (string.IsNullOrEmpty(servicioDTO.Descripcion)) return BadRequest("La descripción es obligatoria.");
            if (servicioDTO.Costo < 0) return BadRequest("El costo no puede ser negativo.");
            if (servicioDTO.Descripcion.Length > 255) return BadRequest("La descripción no puede exceder los 255 caracteres.");

            var reserva = await _appDBcontext.Reservas.FindAsync(servicioDTO.ReservaId);
            if (reserva == null)
                return NotFound("La reserva especificada no existe.");

            // Validación de descripción duplicada
            var descripcionDuplicada = await _appDBcontext.ServiciosAdicionales
                .AnyAsync(s => s.Descripcion == servicioDTO.Descripcion && s.ReservaId == servicioDTO.ReservaId);
            if (descripcionDuplicada)
                return BadRequest("Ya existe un servicio adicional con la misma descripción para esta reserva.");

            var servicio = new AppDBContext.ServicioAdicional
            {
                Descripcion = servicioDTO.Descripcion,
                Costo = servicioDTO.Costo,
                ReservaId = servicioDTO.ReservaId
            };

            _appDBcontext.ServiciosAdicionales.Add(servicio);
            await _appDBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetServicio), new { ServicioAdicionalId = servicio.ServicioAdicionalId }, servicio);
        }

        [HttpPatch("{ServicioAdicionalId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchServicio(int ServicioAdicionalId, ServicioAdicionalUpdateDTO servicioDTO)
        {
            if (servicioDTO == null) return BadRequest("El servicio adicional no puede ser nulo.");

            var servicioExistente = await _appDBcontext.ServiciosAdicionales.FindAsync(ServicioAdicionalId);
            if (servicioExistente == null) return NotFound("Servicio adicional no encontrado.");

            if (servicioDTO.ReservaId.HasValue)
            {
                var reserva = await _appDBcontext.Reservas.FindAsync(servicioDTO.ReservaId.Value);
                if (reserva == null)
                    return NotFound("La reserva especificada no existe.");
                servicioExistente.ReservaId = servicioDTO.ReservaId.Value;
            }

            if (!string.IsNullOrEmpty(servicioDTO.Descripcion))
            {
                if (servicioDTO.Descripcion.Length > 255)
                    return BadRequest("La descripción no puede exceder los 255 caracteres.");

                // Validación de descripción duplicada
                var descripcionDuplicada = await _appDBcontext.ServiciosAdicionales
                    .AnyAsync(s => s.Descripcion == servicioDTO.Descripcion && s.ReservaId == servicioExistente.ReservaId);
                if (descripcionDuplicada)
                    return BadRequest("Ya existe un servicio adicional con la misma descripción para esta reserva.");

                servicioExistente.Descripcion = servicioDTO.Descripcion;
            }

            if (servicioDTO.Costo.HasValue)
            {
                if (servicioDTO.Costo < 0)
                    return BadRequest("El costo no puede ser negativo.");
                servicioExistente.Costo = servicioDTO.Costo.Value;
            }

            await _appDBcontext.SaveChangesAsync();
            return Ok(servicioExistente);
        }

        [HttpDelete("{ServicioAdicionalId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteServicio(int ServicioAdicionalId)
        {
            var servicio = await _appDBcontext.ServiciosAdicionales.FindAsync(ServicioAdicionalId);
            if (servicio == null) return NotFound("Servicio adicional no encontrado.");

            _appDBcontext.ServiciosAdicionales.Remove(servicio);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Servicio eliminado: {servicio.ServicioAdicionalId}");
        }
    }
}
