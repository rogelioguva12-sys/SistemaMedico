using System.ComponentModel.DataAnnotations;

namespace SistemaMedico.DTOs
{
    public class RegistrarConsultaDto
    {
        [Required(ErrorMessage = "El ID del paciente es obligatorio.")]
        public int PacienteId { get; set; }

        [Required(ErrorMessage = "El ID del médico es obligatorio.")]
        public int MedicoId { get; set; }

        [StringLength(255, ErrorMessage = "El motivo no puede exceder 255 caracteres.")]
        public string? Motivo { get; set; }

        public string? Sintomas { get; set; }

        public string? Diagnostico { get; set; }

        public string? Tratamiento { get; set; }

        public string? Observaciones { get; set; }

        [RegularExpression("^(baja|media|alta|urgente)$", ErrorMessage = "La prioridad debe ser: baja, media, alta o urgente.")]
        public string Prioridad { get; set; } = "media";
    }
}
