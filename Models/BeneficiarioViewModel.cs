using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

public class BeneficiarioViewModel : IValidatableObject
{

    public int Id_Beneficiario { get; set; }

    //[Required(ErrorMessage = "El nombre es obligatorio")]
    //[StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;

    //public string Correo { get; set; }

    // ------------------  DUI  ------------------
    [Required(ErrorMessage = "El DUI es obligatorio")]
    [StringLength(10, ErrorMessage = "El DUI debe tener 10 caracteres")]
    [RegularExpression(@"^\d{8}-\d{1}$", ErrorMessage = "Formato de DUI inválido. Use: 12345678-9")]
    [Display(Name = "Número de DUI")]
    public string Dui { get; set; } = string.Empty;

    // ------------------  DIRECCIÓN  ------------------
    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "La dirección debe tener entre 10 y 200 caracteres")]
    [Display(Name = "Dirección Completa")]
    public string Direccion { get; set; } = string.Empty;

    // ------------------  TELÉFONO  ------------------
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(9, MinimumLength = 8, ErrorMessage = "El teléfono debe tener 8 o 9 dígitos")]
    [RegularExpression(@"^[267]\d{7,8}$", ErrorMessage = "Formato de teléfono inválido")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    // ------------------  GÉNERO  ------------------
    [Required(ErrorMessage = "Seleccione un género")]
    [Display(Name = "Género")]
    public string Genero { get; set; } = string.Empty;

    // ------------------  FECHA DE NACIMIENTO  ------------------
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Nacimiento")]
    public DateTime FechaNacimiento { get; set; } = DateTime.Now;

    // ------------------  OTROS CAMPOS  ------------------
    [Display(Name = "Entidad (Opcional)")]
    public int? EntidadId { get; set; }

    [Display(Name = "Estado del Subsidio")]
    public string EstadoSubsidio { get; set; } = "Pendiente";

    // SOLUCIÓN: Hacer el campo opcional y quitar la validación Required para edición
    [Display(Name = "Acepto los términos y condiciones")]
    public bool AceptaTerminos { get; set; }
    public string? EntidadNombre { get; set; }

    // ============================================================
    // VALIDACIÓN REAL DEL DUI (módulo 11)
    // ============================================================

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Normalizar
        var duiNormalized = (Dui ?? "").Trim();

        if (string.IsNullOrEmpty(duiNormalized))
        {
            yield return new ValidationResult("El DUI es obligatorio.", new[] { nameof(Dui) });
            yield break;
        }

        if (!Regex.IsMatch(duiNormalized, @"^\d{8}-\d{1}$"))
        {
            yield return new ValidationResult("Formato de DUI inválido. Use: 12345678-9", new[] { nameof(Dui) });
            yield break;
        }

        if (!EsDuiValido(duiNormalized))
        {
            yield return new ValidationResult("El DUI ingresado no es válido.", new[] { nameof(Dui) });
        }
    }

    private bool EsDuiValido(string dui)
    {
        // espera formato 12345678-9
        var partes = dui.Split('-');
        if (partes.Length != 2) return false;

        var numeros = partes[0];
        if (numeros.Length != 8 || !numeros.All(char.IsDigit)) return false;

        if (!int.TryParse(partes[1], out var verificadorIngresado)) return false;

        int[] pesos = { 9, 8, 7, 6, 5, 4, 3, 2 };
        int suma = 0;
        for (int i = 0; i < 8; i++)
            suma += (numeros[i] - '0') * pesos[i];

        int residuo = suma % 11;
        int calculado = 11 - residuo;

        // reglas del RNPN: si da 10 u 11 -> 0
        if (calculado == 10 || calculado == 11) calculado = 0;

        return calculado == verificadorIngresado;
    }
    public List<Subsidio> Subsidios { get; set; } = new List<Subsidio>();
}