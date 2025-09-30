using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaSubsidios.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaSubsidios_CASATIC.Controllers
{
    [Authorize] // Esto protege todas las acciones dentro del HomeController, si quieres que sea solo Index, ponlo en la acción
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Pasar el nombre del usuario a la vista a través de ViewData o ViewBag
            ViewData["UsuarioNombre"] = User.Identity?.Name ?? "Invitado";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous] // Permite que todos vean la página de error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
