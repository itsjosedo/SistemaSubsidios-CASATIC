using System.ComponentModel.DataAnnotations;

public class Subsidio
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Programa")]
    public string? NombrePrograma { get; set; }

    [Required]
    [Display(Name = "Tipo de Subsidio")]
    public string Tipo { get; set; } = "Canasta básica"; // Ej: Canasta, Beca, Vale transporte

    [Required]
    [Display(Name = "Monto")]
    [DisplayFormat(DataFormatString = "{0:0.00}")] // ← Esto fuerza 2 decimales
    public decimal Monto { get; set; }

    [Required]
    [Display(Name = "Fecha de Asignación")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")] // ← Formato de fecha
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Pendiente";

    // Relación con Beneficiario
    [Required]
    public int BeneficiarioId { get; set; }
    public Beneficiario? Beneficiario { get; set; }
}
