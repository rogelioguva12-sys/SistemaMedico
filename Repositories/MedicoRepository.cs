using System.Data;
using Dapper;
using SistemaMedico.Data;
using SistemaMedico.Models;
using SistemaMedico.Repositories.Interfaces;

namespace SistemaMedico.Repositories
{
    public class MedicoRepository : IMedicoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MedicoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Medico?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"
                SELECT id, cedula, nombre_completo AS NombreCompleto, 
                       especialidad, telefono, correo, activo
                FROM medicos 
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Medico>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Medico>> ObtenerTodosAsync()
        {
            const string sql = @"
                SELECT id, cedula, nombre_completo AS NombreCompleto, 
                       especialidad, telefono, correo, activo
                FROM medicos 
                WHERE activo = TRUE;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Medico>(sql);
        }
    }
}
