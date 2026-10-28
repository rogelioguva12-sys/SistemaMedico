using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaMedico.DTOs;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Endpoint 1: Permite registrar credenciales y asignar el rol correspondiente (Médico, Administrativo o Paciente).
        /// </summary>
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.RegistrarAsync(dto);
                return StatusCode(StatusCodes.Status201Created, new
                {
                    estatus = "exito",
                    mensaje = "Usuario registrado correctamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    estatus = "error",
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint 2: Autentica las credenciales del usuario y genera el token de acceso JWT.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var token = await _authService.LoginAsync(dto);
                return Ok(new
                {
                    estatus = "exito",
                    mensaje = "Inicio de sesión exitoso",
                    token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    estatus = "error",
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint 3: Permite obtener los datos de perfil del usuario a partir del token adjuntado en Authorization: Bearer.
        /// </summary>
        [HttpGet("perfil")]
        [Authorize]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Perfil()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioId))
            {
                return Unauthorized(new
                {
                    estatus = "error",
                    mensaje = "Token no válido o identificador de usuario no encontrado."
                });
            }

            var perfil = await _authService.ObtenerPerfilAsync(usuarioId);
            if (perfil == null)
            {
                return NotFound(new
                {
                    estatus = "error",
                    mensaje = "Usuario no encontrado."
                });
            }

            return Ok(new
            {
                estatus = "exito",
                data = perfil
            });
        }
    }
}
