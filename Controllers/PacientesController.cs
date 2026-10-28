using Microsoft.AspNetCore.Mvc;
using SistemaMedico.DTOs;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PacienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos()
        {
            var pacientes = await _pacienteService.ObtenerTodosAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var paciente = await _pacienteService.ObtenerPorIdAsync(id);
            if (paciente == null)
            {
                return NotFound(new { mensaje = $"Paciente con ID {id} no encontrado." });
            }

            return Ok(paciente);
        }
    }
}
