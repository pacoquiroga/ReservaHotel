using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReservasHotel.DTOs;
using ReservasHotel.Helpers;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _appDBcontext.Clientes.ToListAsync();
            return Ok(clientes);
        }

        [HttpGet("{ClienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppDBContext.Cliente>> GetCliente(int ClienteId)
        {
            var cliente = await _appDBcontext.Clientes.FindAsync(ClienteId);
            if (cliente == null) return NotFound("Cliente no encontrado.");
            return Ok(cliente);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppDBContext.Cliente>> PostCliente(ClienteCreateDto clienteDTO)
        {
            if (clienteDTO == null) return BadRequest("El cliente no puede ser nulo.");
            if (string.IsNullOrEmpty(clienteDTO.Nombre)) return BadRequest("El nombre es obligatorio.");
            if (string.IsNullOrEmpty(clienteDTO.Apellido)) return BadRequest("El apellido es obligatorio.");
            if (clienteDTO.Nombre.Length > 50) return BadRequest("El nombre no puede exceder los 50 caracteres.");
            if (clienteDTO.Apellido.Length > 50) return BadRequest("El apellido no puede exceder los 50 caracteres.");

            // Validación de email único
            if (!string.IsNullOrEmpty(clienteDTO.Email))
            {
                var emailExistente = await _appDBcontext.Clientes
                    .AnyAsync(c => c.Email == clienteDTO.Email);
                if (emailExistente) return BadRequest("El email ya está registrado.");
            }

            // Validación de formato de teléfono
            if (!string.IsNullOrEmpty(clienteDTO.Telefono) && !RegexHelpers.PhoneRegex().IsMatch(clienteDTO.Telefono))
                return BadRequest("El formato del teléfono no es válido.");

            var cliente = new AppDBContext.Cliente
            {
                Nombre = clienteDTO.Nombre,
                Apellido = clienteDTO.Apellido,
                Telefono = clienteDTO.Telefono,
                Email = clienteDTO.Email
            };

            _appDBcontext.Clientes.Add(cliente);
            await _appDBcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCliente), new { ClienteId = cliente.ClienteId }, cliente);
        }

        [HttpPatch("{ClienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchCliente(int ClienteId, ClienteUpdateDto clienteDTO)
        {
            if (clienteDTO == null) return BadRequest("El cliente no puede ser nulo.");

            var clienteExistente = await _appDBcontext.Clientes.FindAsync(ClienteId);
            if (clienteExistente == null) return NotFound("Cliente no encontrado.");

            if (!string.IsNullOrEmpty(clienteDTO.Nombre))
            {
                if (clienteDTO.Nombre.Length > 50)
                    return BadRequest("El nombre no puede exceder los 50 caracteres.");
                clienteExistente.Nombre = clienteDTO.Nombre;
            }

            if (!string.IsNullOrEmpty(clienteDTO.Apellido))
            {
                if (clienteDTO.Apellido.Length > 50)
                    return BadRequest("El apellido no puede exceder los 50 caracteres.");
                clienteExistente.Apellido = clienteDTO.Apellido;
            }

            if (!string.IsNullOrEmpty(clienteDTO.Telefono))
            {
                if (!RegexHelpers.PhoneRegex().IsMatch(clienteDTO.Telefono))
                    return BadRequest("El formato del teléfono no es válido.");
                clienteExistente.Telefono = clienteDTO.Telefono;
            }

            if (!string.IsNullOrEmpty(clienteDTO.Email))
            {
                // Validación de email único
                var emailExistente = await _appDBcontext.Clientes
                    .AnyAsync(c => c.Email == clienteDTO.Email && c.ClienteId != ClienteId);
                if (emailExistente) return BadRequest("El email ya está registrado.");
                clienteExistente.Email = clienteDTO.Email;
            }

            await _appDBcontext.SaveChangesAsync();
            return Ok(clienteExistente);
        }

        [HttpDelete("{ClienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCliente(int ClienteId)
        {
            var cliente = await _appDBcontext.Clientes.FindAsync(ClienteId);
            if (cliente == null) return NotFound("Cliente no encontrado.");

            // Validar que el cliente no tenga reservas futuras
            var reservasFuturas = await _appDBcontext.Reservas
                .AnyAsync(r => r.ClienteId == ClienteId && r.FechaFin > DateTime.Now);
            if (reservasFuturas)
                return BadRequest("El cliente no puede ser eliminado porque tiene reservas futuras.");

            _appDBcontext.Clientes.Remove(cliente);
            await _appDBcontext.SaveChangesAsync();
            return Ok($"Cliente eliminado: {cliente.Nombre} {cliente.Apellido}");
        }
    }
}
