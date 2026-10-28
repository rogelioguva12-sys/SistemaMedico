namespace SistemaMedico.DTOs
{
    public class HistorialPacienteDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Motivo { get; set; }
        public string? Diagnostico { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
    }
}
