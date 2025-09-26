using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoSubsidios.Pages.Home
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public string? UsuarioNombre { get; set; }

        // Método que se ejecuta cuando la página se carga (GET)
        public void OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Si el usuario está autenticado, obtenemos su nombre
                UsuarioNombre = User.Identity.Name ?? "Usuario no autenticado";
            }
            else
            {
                UsuarioNombre = "Invitado";
            }
        }
    }
}


