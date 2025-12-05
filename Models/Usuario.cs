using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Usuario
{
    [Key]
    public int Id_Usuario { get; set; }
    public string? Nombre { get; set; }
    public string? Correo { get; set; }


    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
    ErrorMessage = "La contraseña debe tener al menos una mayúscula, un número y un carácter especial.")]
    [DataType(DataType.Password)]
    public string? Contrasena { get; set; }
    public string? Rol { get; set; } // administrador, operador, beneficiario
    public string Estado { get; set; } = "activo";

    public Entidad? Entidad { get; set; } // relación opcional
    public ICollection<Notificacion>? Notificaciones { get; set; }

    public virtual Beneficiario? Beneficiario { get; set; }
    //public ICollection<Beneficiario>? Beneficiarios { get; set; }
}