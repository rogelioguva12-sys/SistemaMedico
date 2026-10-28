using System.Data;
using Dapper;
using SistemaMedico.Data;
using SistemaMedico.DTOs;
using SistemaMedico.Models;
using SistemaMedico.Repositories.Interfaces;

namespace SistemaMedico.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UsuarioRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            const string sql = @"
                SELECT id, 
                       nombre_completo AS NombreCompleto, 
                       correo, 
                       password_hash AS PasswordHash, 
                       rol, 
                       telefono, 
                       fecha_registro AS FechaRegistro
                FROM usuarios 
                WHERE correo = @Correo;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Correo = correo });
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"
                SELECT id, 
                       nombre_completo AS NombreCompleto, 
                       correo, 
                       password_hash AS PasswordHash, 
                       rol, 
                       telefono, 
                       fecha_registro AS FechaRegistro
                FROM usuarios 
                WHERE id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
        }

        public async Task<PerfilUsuarioDto?> ObtenerPerfilPorIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    u.id AS Id,
                    u.nombre_completo AS NombreCompleto,
                    u.correo AS Correo,
                    u.rol AS Rol,
                    m.cedula AS Cedula
                FROM usuarios u
                LEFT JOIN medicos m ON u.correo = m.correo
                WHERE u.id = @Id;";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<PerfilUsuarioDto>(sql, new { Id = id });
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            const string sql = "SELECT COUNT(1) FROM usuarios WHERE correo = @Correo;";
            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Correo = correo });
            return count > 0;
        }

        public async Task<bool> ExisteCedulaAsync(string cedula)
        {
            const string sql = "SELECT COUNT(1) FROM medicos WHERE cedula = @Cedula;";
            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Cedula = cedula });
            return count > 0;
        }

        public async Task<int> RegistrarUsuarioAsync(Usuario usuario, string? cedula)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sqlUsuario = @"
                    INSERT INTO usuarios (nombre_completo, correo, password_hash, rol, telefono, fecha_registro)
                    VALUES (@NombreCompleto, @Correo, @PasswordHash, @Rol, @Telefono, NOW());
                    SELECT LAST_INSERT_ID();";

                var usuarioId = await connection.ExecuteScalarAsync<int>(sqlUsuario, usuario, transaction);
                usuario.Id = usuarioId;

                // Si el rol es Medico y se proporcionó cédula, vincular o insertar en la tabla medicos
                if (string.Equals(usuario.Rol, "Medico", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(cedula))
                {
                    const string sqlExisteMedico = "SELECT COUNT(1) FROM medicos WHERE correo = @Correo;";
                    var existeMedico = await connection.ExecuteScalarAsync<int>(sqlExisteMedico, new { usuario.Correo }, transaction);

                    if (existeMedico > 0)
                    {
                        const string sqlUpdateMedico = @"
                            UPDATE medicos 
                            SET cedula = @Cedula, 
                                nombre_completo = @NombreCompleto, 
                                telefono = @Telefono, 
                                activo = 1 
                            WHERE correo = @Correo;";

                        await connection.ExecuteAsync(sqlUpdateMedico, new
                        {
                            Cedula = cedula,
                            usuario.NombreCompleto,
                            usuario.Telefono,
                            usuario.Correo
                        }, transaction);
                    }
                    else
                    {
                        const string sqlInsertMedico = @"
                            INSERT INTO medicos (cedula, nombre_completo, especialidad, telefono, correo, activo)
                            VALUES (@Cedula, @NombreCompleto, 'Medicina General', @Telefono, @Correo, 1);";

                        await connection.ExecuteAsync(sqlInsertMedico, new
                        {
                            Cedula = cedula,
                            usuario.NombreCompleto,
                            usuario.Telefono,
                            usuario.Correo
                        }, transaction);
                    }
                }

                transaction.Commit();
                return usuarioId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
