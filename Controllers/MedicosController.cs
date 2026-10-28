using Microsoft.AspNetCore.Mvc;
using SistemaMedico.DTOs;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _medicoService;

        public MedicosController(IMedicoService medicoService)
        {
            _medicoService = medicoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos()
        {
            var medicos = await _medicoService.ObtenerTodosAsync();
            return Ok(medicos);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var medico = await _medicoService.ObtenerPorIdAsync(id);
            if (medico == null)
            {
                return NotFound(new { mensaje = $"Médico con ID {id} no encontrado." });
            }

            return Ok(medico);
        }
    }
}
