using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Subsidio
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Programa")]
    public string? NombrePrograma { get; set; }

    [Required]
    [Display(Name = "Tipo de Subsidio")]
    public string Tipo { get; set; } = "Canasta básica";

    [Required]
    [Display(Name = "Monto")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public decimal Monto { get; set; }

    [Required]
    [Display(Name = "Fecha de Asignación")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Pendiente";

    // 🔹 MARCAR como No Mapeada (solo para compatibilidad temporal)
    [NotMapped]
    public int? BeneficiarioId { get; set; }
    
    [NotMapped]
    public Beneficiario? Beneficiario { get; set; }

    // 🔹 RELACIÓN NUEVA muchos-a-muchos
    public virtual ICollection<Beneficiario> Beneficiarios { get; set; } = new List<Beneficiario>();
}