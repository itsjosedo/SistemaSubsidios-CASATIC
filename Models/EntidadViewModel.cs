using System.ComponentModel.DataAnnotations;

public class EntidadViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    public string Direccion { get; set; }  
}