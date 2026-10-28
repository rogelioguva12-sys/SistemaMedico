using SistemaMedico.DTOs;
using SistemaMedico.Models;

namespace SistemaMedico.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<PerfilUsuarioDto?> ObtenerPerfilPorIdAsync(int id);
        Task<int> RegistrarUsuarioAsync(Usuario usuario, string? cedula);
        Task<bool> ExisteCorreoAsync(string correo);
        Task<bool> ExisteCedulaAsync(string cedula);
    }
}
