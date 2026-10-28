using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaMedico.DTOs;
using SistemaMedico.Models;
using SistemaMedico.Repositories.Interfaces;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        private static readonly HashSet<string> RolesValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "Medico",
            "Administrativo",
            "Paciente"
        };

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task RegistrarAsync(RegistroUsuarioDto dto)
        {
            // Validar que el rol sea uno de los permitidos
            if (string.IsNullOrWhiteSpace(dto.Rol) || !RolesValidos.Contains(dto.Rol.Trim()))
            {
                throw new ArgumentException("El rol especificado no es válido. Debe ser Médico, Administrativo o Paciente.");
            }

            // Normalizar el nombre del rol a formato estándar
            var rolNormalizado = dto.Rol.Trim().ToLowerInvariant() switch
            {
                "medico" or "médico" => "Medico",
                "administrativo" => "Administrativo",
                "paciente" => "Paciente",
                _ => dto.Rol.Trim()
            };

            // Validar cédula obligatoria para médicos
            if (rolNormalizado == "Medico")
            {
                if (string.IsNullOrWhiteSpace(dto.Cedula))
                {
                    throw new ArgumentException("La cédula profesional es requerida para el rol de Médico.");
                }

                if (await _usuarioRepository.ExisteCedulaAsync(dto.Cedula.Trim()))
                {
                    throw new InvalidOperationException("La cédula profesional ya se encuentra registrada en el sistema.");
                }
            }

            // Validar que el correo no esté duplicado
            if (await _usuarioRepository.ExisteCorreoAsync(dto.Correo.Trim()))
            {
                throw new InvalidOperationException("El correo ya se encuentra registrado en el sistema.");
            }

            // Hashear contraseña con BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto.Trim(),
                Correo = dto.Correo.Trim(),
                PasswordHash = passwordHash,
                Rol = rolNormalizado,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                FechaRegistro = DateTime.UtcNow
            };

            await _usuarioRepository.RegistrarUsuarioAsync(nuevoUsuario, dto.Cedula?.Trim());
        }

        public async Task<string> LoginAsync(LoginUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo.Trim());
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");
            }

            bool passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            if (!passwordValida)
            {
                throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");
            }

            return GenerarTokenJwt(usuario);
        }

        public async Task<PerfilUsuarioDto?> ObtenerPerfilAsync(int usuarioId)
        {
            return await _usuarioRepository.ObtenerPerfilPorIdAsync(usuarioId);
        }

        private string GenerarTokenJwt(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings.GetValue<string>("Secret") 
                ?? throw new InvalidOperationException("La clave secreta de JWT ('Jwt:Secret') no está configurada.");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.NombreCompleto),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol),
                new("id", usuario.Id.ToString()),
                new("correo", usuario.Correo),
                new("rol", usuario.Rol),
                new("nombreCompleto", usuario.NombreCompleto)
            };

            var expirationHours = jwtSettings.GetValue<int?>("ExpirationInHours") ?? 4;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
