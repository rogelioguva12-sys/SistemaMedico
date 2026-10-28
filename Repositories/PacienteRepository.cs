using System.Data;
using Dapper;
using SistemaMedico.Data;
using SistemaMedico.Models;
using SistemaMedico.Repositories.Interfaces;

namespace SistemaMedico.Repositories
{
    public class PacienteRepository : IPacienteRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PacienteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Paciente?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"
                SELECT id, matricula, nombre_completo AS NombreCompleto, 
                       fecha_nacimiento AS FechaNacimiento, telefono, correo, 
                       carrera, tipo_sangre AS TipoSangre, alergias, activo
                FROM pacientes 
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Paciente>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Paciente>> ObtenerTodosAsync()
        {
            const string sql = @"
                SELECT id, matricula, nombre_completo AS NombreCompleto, 
                       fecha_nacimiento AS FechaNacimiento, telefono, correo, 
                       carrera, tipo_sangre AS TipoSangre, alergias, activo
                FROM pacientes 
                WHERE activo = TRUE;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Paciente>(sql);
        }
    }
}
