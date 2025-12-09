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
    [Display(Name = "Fecha de Expiración")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddYears(1);

    [Display(Name = "Fecha de Última Renovación")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? FechaRenovacion { get; set; }

    [Display(Name = "Próxima Renovación")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? ProximaRenovacion { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Pendiente";

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;

    [Display(Name = "Fecha Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario que Eliminó")]
    [StringLength(100)]
    public string? UsuarioEliminacionId { get; set; }

    [Display(Name = "Motivo Eliminación")]
    [StringLength(500)]
    public string? MotivoEliminacion { get; set; }

    [NotMapped]
    [Display(Name = "Días Restantes")]
    public int DiasRestantes 
    { 
        get 
        {
            var dias = (FechaExpiracion - DateTime.Now).Days;
            return dias > 0 ? dias : 0;
        }
    }

    [NotMapped]
    [Display(Name = "Próximo a Expirar")]
    public bool ProximoAExpirar 
    { 
        get 
        {
            return DiasRestantes <= 30 && DiasRestantes > 0 && Estado == "Activo";
        }
    }

    [NotMapped]
    [Display(Name = "Expirado")]
    public bool Expirado 
    { 
        get 
        {
            return FechaExpiracion < DateTime.Now && Estado == "Activo";
        }
    }

    [Required]
    [Display(Name = "Usuario Creación")]
    public string UsuarioCreacionId { get; set; } = string.Empty;

    [NotMapped]
    public int? BeneficiarioId { get; set; }
    
    [NotMapped]
    public Beneficiario? Beneficiario { get; set; }

    // RELACIÓN NUEVA muchos-a-muchos
    public virtual ICollection<Beneficiario> Beneficiarios { get; set; } = new List<Beneficiario>();


    [NotMapped]
    [Display(Name = "Estado General")]
    public string EstadoGeneral
    {
        get
        {
            if (Eliminado) return "Eliminado";
            if (Expirado) return "Expirado";
            if (ProximoAExpirar) return "Por Expirar";
            return Estado;
        }
    }
}