using System.ComponentModel.DataAnnotations;

namespace SistemaMedico.DTOs
{
    public class RegistroUsuarioDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del correo es inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido.")]
        public string Rol { get; set; } = string.Empty; // "Medico", "Administrativo", "Paciente"

        public string? Cedula { get; set; }

        public string? Telefono { get; set; }
    }
}
