using SistemaMedico.DTOs;
using SistemaMedico.Repositories.Interfaces;
using SistemaMedico.Services.Interfaces;

namespace SistemaMedico.Services
{
    public class ConsultaService : IConsultaService
    {
        private readonly IConsultaRepository _consultaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMedicoRepository _medicoRepository;

        public ConsultaService(
            IConsultaRepository consultaRepository,
            IPacienteRepository pacienteRepository,
            IMedicoRepository medicoRepository)
        {
            _consultaRepository = consultaRepository;
            _pacienteRepository = pacienteRepository;
            _medicoRepository = medicoRepository;
        }

        public async Task<ConsultaDetalleDto> RegistrarConsultaAsync(RegistrarConsultaDto dto)
        {
            // Regla de Negocio: Verificar que el paciente exista y esté activo
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(dto.PacienteId);
            if (paciente == null || !paciente.Activo)
            {
                throw new KeyNotFoundException($"El paciente con ID {dto.PacienteId} no existe o no se encuentra activo.");
            }

            // Regla de Negocio: Verificar que el médico exista y esté activo
            var medico = await _medicoRepository.ObtenerPorIdAsync(dto.MedicoId);
            if (medico == null || !medico.Activo)
            {
                throw new KeyNotFoundException($"El médico con ID {dto.MedicoId} no existe o no se encuentra activo.");
            }

            // Ejecutar el procedimiento almacenado sp_registrar_consulta
            int nuevaConsultaId = await _consultaRepository.RegistrarConsultaAsync(dto);

            // Obtener el registro completo de la consulta recién creada
            var consulta = await _consultaRepository.ObtenerPorIdAsync(nuevaConsultaId);
            if (consulta == null)
            {
                throw new InvalidOperationException("Error al recuperar la consulta registrada.");
            }

            return MapToConsultaDetalleDto(consulta);
        }

        public async Task<ConsultaDetalleDto?> ObtenerPorIdAsync(int consultaId)
        {
            var consulta = await _consultaRepository.ObtenerPorIdAsync(consultaId);
            if (consulta == null) return null;

            return MapToConsultaDetalleDto(consulta);
        }

        public async Task<IEnumerable<HistorialPacienteDto>> ObtenerHistorialPorPacienteAsync(int pacienteId)
        {
            // Validar existencia previa del paciente
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(pacienteId);
            if (paciente == null)
            {
                throw new KeyNotFoundException($"El paciente con ID {pacienteId} no existe.");
            }

            return await _consultaRepository.ObtenerHistorialPorPacienteAsync(pacienteId);
        }

        public async Task<MedicoDto?> ObtenerMedicoPorConsultaAsync(int consultaId)
        {
            var medico = await _consultaRepository.ObtenerMedicoPorConsultaAsync(consultaId);
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

        public async Task<PacienteDto?> ObtenerPacientePorConsultaAsync(int consultaId)
        {
            var paciente = await _consultaRepository.ObtenerPacientePorConsultaAsync(consultaId);
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

        private static ConsultaDetalleDto MapToConsultaDetalleDto(Models.Consulta c)
        {
            return new ConsultaDetalleDto
            {
                Id = c.Id,
                PacienteId = c.PacienteId,
                MedicoId = c.MedicoId,
                FechaHora = c.FechaHora,
                Motivo = c.Motivo,
                Sintomas = c.Sintomas,
                Diagnostico = c.Diagnostico,
                Tratamiento = c.Tratamiento,
                Observaciones = c.Observaciones,
                Prioridad = c.Prioridad,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}
