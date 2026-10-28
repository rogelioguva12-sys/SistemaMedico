using SistemaMedico.Models;

namespace SistemaMedico.Repositories.Interfaces
{
    public interface IPacienteRepository
    {
        Task<Paciente?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Paciente>> ObtenerTodosAsync();
    }
}
