using SistemaMedico.DTOs;

namespace SistemaMedico.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegistrarAsync(RegistroUsuarioDto dto);
        Task<string> LoginAsync(LoginUsuarioDto dto);
        Task<PerfilUsuarioDto?> ObtenerPerfilAsync(int usuarioId);
    }
}
