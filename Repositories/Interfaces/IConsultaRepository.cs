using SistemaMedico.DTOs;
using SistemaMedico.Models;

namespace SistemaMedico.Repositories.Interfaces
{
    public interface IConsultaRepository
    {
        Task<int> RegistrarConsultaAsync(RegistrarConsultaDto dto);
        Task<Consulta?> ObtenerPorIdAsync(int consultaId);
        Task<IEnumerable<HistorialPacienteDto>> ObtenerHistorialPorPacienteAsync(int pacienteId);
        Task<Medico?> ObtenerMedicoPorConsultaAsync(int consultaId);
        Task<Paciente?> ObtenerPacientePorConsultaAsync(int consultaId);
    }
}
