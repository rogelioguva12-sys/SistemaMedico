using SistemaMedico.DTOs;

namespace SistemaMedico.Services.Interfaces
{
    public interface IPacienteService
    {
        Task<PacienteDto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<PacienteDto>> ObtenerTodosAsync();
    }
}
