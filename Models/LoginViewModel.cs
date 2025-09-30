using System.ComponentModel.DataAnnotations;

namespace SistemaSubsidios_CASATIC.Models
{
   public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo invalido")]
        public string Correo { get; set; }
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }
    }
 
}