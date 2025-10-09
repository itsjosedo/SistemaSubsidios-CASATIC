using SistemaSubsidios_CASATIC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BeneficiariosController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BeneficiariosController> _logger;

        public BeneficiariosController(AppDbContext context, ILogger<BeneficiariosController> logger)
        {
            _context = context; 
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {

            var userId = GetUserId();
            ViewBag.UserId = userId;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .FirstOrDefaultAsync(b => b.Id_Beneficiario == userId.Value);

            if (beneficiario == null)
            {
                ViewBag.Mensaje = "No se encontró información para este usuario.";
                return View();
            }

            // Mapear a ViewModel
            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Direccion = beneficiario.Direccion,
                Telefono = beneficiario.Telefono,
                EntidadId = beneficiario.EntidadId,
                EstadoSubsidio = beneficiario.EstadoSubsidio,
                EntidadNombre = beneficiario.Entidad?.Nombre
            };

            return View(model);
        }

    }
}