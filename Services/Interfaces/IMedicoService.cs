using SistemaMedico.DTOs;

namespace SistemaMedico.Services.Interfaces
{
    public interface IMedicoService
    {
        Task<MedicoDto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<MedicoDto>> ObtenerTodosAsync();
    }
}
