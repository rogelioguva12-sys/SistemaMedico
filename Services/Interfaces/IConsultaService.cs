using SistemaMedico.DTOs;

namespace SistemaMedico.Services.Interfaces
{
    public interface IConsultaService
    {
        Task<ConsultaDetalleDto> RegistrarConsultaAsync(RegistrarConsultaDto dto);
        Task<ConsultaDetalleDto?> ObtenerPorIdAsync(int consultaId);
        Task<IEnumerable<HistorialPacienteDto>> ObtenerHistorialPorPacienteAsync(int pacienteId);
        Task<MedicoDto?> ObtenerMedicoPorConsultaAsync(int consultaId);
        Task<PacienteDto?> ObtenerPacientePorConsultaAsync(int consultaId);
    }
}
