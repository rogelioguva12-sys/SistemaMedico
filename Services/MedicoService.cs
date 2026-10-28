using SistemaMedico.DTOs;
using SistemaMedico.Repositories.Interfaces;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly IMedicoRepository _medicoRepository;

        public MedicoService(IMedicoRepository medicoRepository)
        {
            _medicoRepository = medicoRepository;
        }

        public async Task<MedicoDto?> ObtenerPorIdAsync(int id)
        {
            var medico = await _medicoRepository.ObtenerPorIdAsync(id);
            if (medico == null) return null;

            return new MedicoDto
            {
                Id = medico.Id,
                Cedula = medico.Cedula,
                NombreCompleto = medico.NombreCompleto,
                Especialidad = medico.Especialidad,
                Telefono = medico.Telefono,
                Correo = medico.Correo,
                Activo = medico.Activo
            };
        }

        public async Task<IEnumerable<MedicoDto>> ObtenerTodosAsync()
        {
            var medicos = await _medicoRepository.ObtenerTodosAsync();
            return medicos.Select(m => new MedicoDto
            {
                Id = m.Id,
                Cedula = m.Cedula,
                NombreCompleto = m.NombreCompleto,
                Especialidad = m.Especialidad,
                Telefono = m.Telefono,
                Correo = m.Correo,
                Activo = m.Activo
            });
        }
    }
}
