using SistemaMedico.DTOs;
using SistemaMedico.Repositories.Interfaces;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;

        public PacienteService(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository;
        }

        public async Task<PacienteDto?> ObtenerPorIdAsync(int id)
        {
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(id);
            if (paciente == null) return null;

            return new PacienteDto
            {
                Id = paciente.Id,
                Matricula = paciente.Matricula,
                NombreCompleto = paciente.NombreCompleto,
                FechaNacimiento = paciente.FechaNacimiento,
                Telefono = paciente.Telefono,
                Correo = paciente.Correo,
                Carrera = paciente.Carrera,
                TipoSangre = paciente.TipoSangre,
                Alergias = paciente.Alergias,
                Activo = paciente.Activo
            };
        }

        public async Task<IEnumerable<PacienteDto>> ObtenerTodosAsync()
        {
            var pacientes = await _pacienteRepository.ObtenerTodosAsync();
            return pacientes.Select(p => new PacienteDto
            {
                Id = p.Id,
                Matricula = p.Matricula,
                NombreCompleto = p.NombreCompleto,
                FechaNacimiento = p.FechaNacimiento,
                Telefono = p.Telefono,
                Correo = p.Correo,
                Carrera = p.Carrera,
                TipoSangre = p.TipoSangre,
                Alergias = p.Alergias,
                Activo = p.Activo
            });
        }
    }
}
