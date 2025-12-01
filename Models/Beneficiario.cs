using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Beneficiario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "ID Beneficiario")]
    public int Id_Beneficiario { get; set; }

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
    [RegularExpression(@"^[267]\d{7,8}$", ErrorMessage = "Formato de teléfono inválido. Debe comenzar con 2, 6 o 7")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;


    [Required(ErrorMessage = "Seleccione un género")]
    [Display(Name = "Género")]
    public string Genero { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Nacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Estado del Subsidio")]
    public string EstadoSubsidio { get; set; } = "pendiente";

    // CAMBIO: Hacer opcional removiendo [Required] y usando int?
    [Display(Name = "Entidad")]
    public int? EntidadId { get; set; }  // ← QUITAR [Required] y usar int?

    [ForeignKey("EntidadId")]
    public virtual Entidad? Entidad { get; set; }
    
    [ForeignKey("UsuarioId")]
    public int? UsuarioId { get; set; } // Ahora es opcional
    public Usuario? Usuario { get; set; } // Ahora puede ser null

//Relación muchos-a-muchos con Subsidio
    public virtual ICollection<Subsidio> Subsidios { get; set; } = new List<Subsidio>();
    // ... el resto del código igual ...
}