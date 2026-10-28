namespace SistemaMedico.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Carrera { get; set; }
        public string? TipoSangre { get; set; }
        public string? Alergias { get; set; }
        public bool Activo { get; set; } = true;
    }
}
