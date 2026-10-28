using System.Text.Json.Serialization;

namespace SistemaMedico.DTOs
{
    public class PerfilUsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Cedula { get; set; }
    }
}
