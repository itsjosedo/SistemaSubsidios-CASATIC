using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SistemaSubsidios_CASATIC.Models
{
    // Esta clase representa una sola fila de tu archivo Excel/CSV
    public class BeneficiarioImportDto
    {
        [Name("Nombre")] // Esto busca una columna llamada "Nombre" en el CSV
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Name("DUI")]
        [Required(ErrorMessage = "El DUI es obligatorio")]
        [RegularExpression(@"^\d{8}-\d{1}$", ErrorMessage = "Formato de DUI inválido (ej: 12345678-9)")]
        public string Dui { get; set; } = string.Empty;

        [Name("Direccion")]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        [Name("Telefono")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^[267]\d{7,8}$", ErrorMessage = "Teléfono inválido (debe iniciar con 2, 6 o 7)")]
        public string Telefono { get; set; } = string.Empty;

        [Name("Genero")]
        [Required(ErrorMessage = "El género es obligatorio")]
        public string Genero { get; set; } = string.Empty;

        // CsvHelper intentará convertir texto a fecha automáticamente si el formato es estándar
        [Name("FechaNacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime? FechaNacimiento { get; set; }
    }
}