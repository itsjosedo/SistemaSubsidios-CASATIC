using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSubsidios_CASATIC.Models;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class SolicitudesController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SolicitudesController> _logger;

        public SolicitudesController(AppDbContext context, ILogger<SolicitudesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Solicitudes/Pendientes
        public async Task<IActionResult> Pendientes()
        {
            var solicitudesPendientes = await _context.Subsidios
                .Include(s => s.Beneficiario)
                .Where(s => s.Estado == "Pendiente" || s.Estado == "En Revisión")
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Solicitudes Pendientes";
            return View(solicitudesPendientes);
        }
    }
}