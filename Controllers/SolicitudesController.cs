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

        // GET: Subsidios/SolicitudesPendientes
        public async Task<IActionResult> Pendientes()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            IQueryable<Subsidio> solicitudesQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => s.Estado == "Pendiente");

            // Filtrar por entidad si es necesario
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                solicitudesQuery = solicitudesQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var solicitudes = await solicitudesQuery
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";
            return View(solicitudes);
        }
    }
}