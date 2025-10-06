using System.ComponentModel.DataAnnotations;

public class EntidadViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }
    public string Direccion { get; set; }  

    // Datos del Usuario administrador
    [Required(ErrorMessage = "El nombre del usuario es obligatorio")]
    public string NombreUsuario { get; set; }

    [Required(ErrorMessage = "El correo del usuario es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string CorreoUsuario { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; }

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [DataType(DataType.Password)]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarContrasena { get; set; }

    
}