using System.ComponentModel.DataAnnotations;

public class EntidadViewModel
{
    public int Id { get; set; } 
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre de Entidad")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [Display(Name = "Dirección Completa")]
    public string Direccion { get; set; }  

    // Datos del Usuario administrador (solo para creación)
    [Display(Name = "Nombre del Usuario")]
    public string? NombreUsuario { get; set; }

    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo del Usuario")]
    public string? CorreoUsuario { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Contrasena { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarContrasena { get; set; }
}