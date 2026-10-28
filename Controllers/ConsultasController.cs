using Microsoft.AspNetCore.Mvc;
using SistemaMedico.DTOs;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConsultasController : ControllerBase
    {
        private readonly IConsultaService _consultaService;

        public ConsultasController(IConsultaService consultaService)
        {
            _consultaService = consultaService;
        }

        /// <summary>
        /// Registra una nueva consulta médica (Invoca sp_registrar_consulta).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ConsultaDetalleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarConsulta([FromBody] RegistrarConsultaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var consultaRegistrada = await _consultaService.RegistrarConsultaAsync(dto);
                return CreatedAtAction(
                    nameof(ObtenerConsultaPorId), 
                    new { id = consultaRegistrada.Id }, 
                    consultaRegistrada
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Consulta el detalle de una consulta por su ID (Invoca sp_consultar_consulta).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ConsultaDetalleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerConsultaPorId(int id)
        {
            var consulta = await _consultaService.ObtenerPorIdAsync(id);
            if (consulta == null)
            {
                return NotFound(new { mensaje = $"No se encontró la consulta médica con ID {id}." });
            }

            return Ok(consulta);
        }

        /// <summary>
        /// Consulta el historial de consultas de un paciente ordenadas de más reciente a más antigua (Invoca sp_historial_paciente).
        /// </summary>
        [HttpGet("paciente/{pacienteId:int}/historial")]
        [ProducesResponseType(typeof(IEnumerable<HistorialPacienteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerHistorialPaciente(int pacienteId)
        {
            try
            {
                var historial = await _consultaService.ObtenerHistorialPorPacienteAsync(pacienteId);
                return Ok(historial);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Identifica el médico que realizó una consulta médica (Invoca sp_medico_de_consulta).
        /// </summary>
        [HttpGet("{id:int}/medico")]
        [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerMedicoDeConsulta(int id)
        {
            var medico = await _consultaService.ObtenerMedicoPorConsultaAsync(id);
            if (medico == null)
            {
                return NotFound(new { mensaje = $"No se encontró un médico asociado a la consulta con ID {id}." });
            }

            return Ok(medico);
        }

        /// <summary>
        /// Identifica el paciente asociado a una consulta médica (Invoca sp_paciente_de_consulta).
        /// </summary>
        [HttpGet("{id:int}/paciente")]
        [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPacienteDeConsulta(int id)
        {
            var paciente = await _consultaService.ObtenerPacientePorConsultaAsync(id);
            if (paciente == null)
            {
                return NotFound(new { mensaje = $"No se encontró un paciente asociado a la consulta con ID {id}." });
            }

            return Ok(paciente);
        }
    }
}
