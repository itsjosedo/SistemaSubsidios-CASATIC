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
        public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddYears(1); // Por defecto 1 año

        [Display(Name = "Fecha de Última Renovación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? FechaRenovacion { get; set; }

        [Display(Name = "Próxima Renovación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ProximaRenovacion { get; set; }


    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Pendiente";

     // 🔹 Propiedades calculadas (NoMapped para no guardar en BD)
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

    // 🔹 NUEVA PROPIEDAD para filtrar por usuario creador
    [Required]
    [Display(Name = "Usuario Creación")]
    public string UsuarioCreacionId { get; set; } = string.Empty;

    // 🔹 MARCAR como No Mapeada (solo para compatibilidad temporal)
    [NotMapped]
    public int? BeneficiarioId { get; set; }
    
    [NotMapped]
    public Beneficiario? Beneficiario { get; set; }

    // 🔹 RELACIÓN NUEVA muchos-a-muchos
    public virtual ICollection<Beneficiario> Beneficiarios { get; set; } = new List<Beneficiario>();
}