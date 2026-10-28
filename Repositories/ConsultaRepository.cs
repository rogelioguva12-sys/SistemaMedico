using System.Data;
using Dapper;
using SistemaMedico.Data;
using SistemaMedico.DTOs;
using SistemaMedico.Models;
using SistemaMedico.Repositories.Interfaces;

namespace SistemaMedico.Repositories
{
    public class ConsultaRepository : IConsultaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ConsultaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> RegistrarConsultaAsync(RegistrarConsultaDto dto)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_paciente_id", dto.PacienteId);
            parameters.Add("p_medico_id", dto.MedicoId);
            parameters.Add("p_motivo", dto.Motivo);
            parameters.Add("p_sintomas", dto.Sintomas);
            parameters.Add("p_diagnostico", dto.Diagnostico);
            parameters.Add("p_tratamiento", dto.Tratamiento);
            parameters.Add("p_observaciones", dto.Observaciones);
            parameters.Add("p_prioridad", dto.Prioridad);

            var consultaId = await connection.ExecuteScalarAsync<int>(
                "sp_registrar_consulta",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return consultaId;
        }

        public async Task<Consulta?> ObtenerPorIdAsync(int consultaId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT id, 
                       paciente_id AS PacienteId, 
                       medico_id AS MedicoId, 
                       fecha_hora AS FechaHora, 
                       motivo, 
                       sintomas, 
                       diagnostico, 
                       tratamiento, 
                       observaciones, 
                       prioridad, 
                       created_at AS CreatedAt, 
                       updated_at AS UpdatedAt
                FROM consultas
                WHERE id = @p_consulta_id;";

            return await connection.QueryFirstOrDefaultAsync<Consulta>(
                sql,
                new { p_consulta_id = consultaId }
            );
        }

        public async Task<IEnumerable<HistorialPacienteDto>> ObtenerHistorialPorPacienteAsync(int pacienteId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_paciente_id", pacienteId);

            return await connection.QueryAsync<HistorialPacienteDto>(
                "sp_historial_paciente",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Medico?> ObtenerMedicoPorConsultaAsync(int consultaId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT m.id, 
                       m.cedula, 
                       m.nombre_completo AS NombreCompleto, 
                       m.especialidad, 
                       m.telefono, 
                       m.correo, 
                       m.activo
                FROM medicos m
                JOIN consultas c ON c.medico_id = m.id
                WHERE c.id = @p_consulta_id;";

            return await connection.QueryFirstOrDefaultAsync<Medico>(
                sql,
                new { p_consulta_id = consultaId }
            );
        }

        public async Task<Paciente?> ObtenerPacientePorConsultaAsync(int consultaId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT p.id, 
                       p.matricula, 
                       p.nombre_completo AS NombreCompleto, 
                       p.fecha_nacimiento AS FechaNacimiento, 
                       p.telefono, 
                       p.correo, 
                       p.carrera, 
                       p.tipo_sangre AS TipoSangre, 
                       p.alergias, 
                       p.activo
                FROM pacientes p
                JOIN consultas c ON c.paciente_id = p.id
                WHERE c.id = @p_consulta_id;";

            return await connection.QueryFirstOrDefaultAsync<Paciente>(
                sql,
                new { p_consulta_id = consultaId }
            );
        }
    }
}
