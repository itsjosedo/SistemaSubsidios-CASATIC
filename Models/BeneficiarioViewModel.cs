using System.ComponentModel.DataAnnotations;

public class BeneficiarioViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DUI es obligatorio")]
    [StringLength(10, ErrorMessage = "El DUI debe tener 10 caracteres")]
    [RegularExpression(@"^\d{8}-\d{1}$", ErrorMessage = "Formato de DUI inválido. Use: 12345678-9")]
    [Display(Name = "Número de DUI")]
    public string Dui { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "La dirección debe tener entre 10 y 200 caracteres")]
    [Display(Name = "Dirección Completa")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(9, MinimumLength = 8, ErrorMessage = "El teléfono debe tener 8 o 9 dígitos")]
    [RegularExpression(@"^[267]\d{7,8}$", ErrorMessage = "Formato de teléfono inválido")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    // HACER OPCIONAL TEMPORALMENTE
    [Display(Name = "Entidad (Opcional)")]
    public int? EntidadId { get; set; }  // ← Cambiado a nullable

    [Display(Name = "Acepto los términos y condiciones")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos y condiciones")]
    public bool AceptaTerminos { get; set; }
}