using Microsoft.AspNetCore.Mvc;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            // Si el usuario está autenticado, redirigir según su rol
            if (User.Identity.IsAuthenticated)
            {
                var rol = GetRolUsuario();
                
                switch (rol?.ToLower())
                {
                    case "entidad":
                    case "operador":
                        return RedirectToAction("Dashboard", "Entidad");
                    case "beneficiario":
                        return RedirectToAction("Create", "Beneficiarios");
                    case "admin":
                    case "administrador":
                        // Solo los administradores ven el Home normal
                        return View();
                    default:
                        return View();
                }
            }
            
            // Si no está autenticado, mostrar el Home normal
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}