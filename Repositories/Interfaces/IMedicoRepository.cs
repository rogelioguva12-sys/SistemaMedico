using SistemaMedico.Models;

namespace SistemaMedico.Repositories.Interfaces
{
    public interface IMedicoRepository
    {
        Task<Medico?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Medico>> ObtenerTodosAsync();
    }
}
