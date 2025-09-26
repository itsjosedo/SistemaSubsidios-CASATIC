using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BeneficiariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BeneficiariosController> _logger;

        public BeneficiariosController(AppDbContext context, ILogger<BeneficiariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Beneficiarios/Create
        public async Task<IActionResult> Create()
        {
            await CargarEntidadesViewBag();
            return View();
        }

        // POST: Beneficiarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BeneficiarioViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await CargarEntidadesViewBag();
                    return View(model);
                }

                // Validar DUI único
                if (await _context.Beneficiarios.AnyAsync(b => b.Dui == model.Dui))
                {
                    ModelState.AddModelError("Dui", "Este DUI ya está registrado en el sistema");
                    await CargarEntidadesViewBag();
                    return View(model);
                }

                // Validar que la entidad existe (solo si se proporciona)
                if (model.EntidadId.HasValue)
                {
                    var entidadExiste = await _context.Entidades.AnyAsync(e => e.Id == model.EntidadId.Value);
                    if (!entidadExiste)
                    {
                        ModelState.AddModelError("EntidadId", "La entidad seleccionada no es válida");
                        await CargarEntidadesViewBag();
                        return View(model);
                    }
                }

                // Crear nuevo beneficiario
                var beneficiario = new Beneficiario
                {
                    Nombre = model.Nombre.Trim(),
                    Dui = model.Dui,
                    Direccion = model.Direccion.Trim(),
                    Telefono = model.Telefono,
                    EntidadId = model.EntidadId,
                    EstadoSubsidio = "pendiente"
                };

                _context.Beneficiarios.Add(beneficiario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Beneficiario registrado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear beneficiario");
                await CargarEntidadesViewBag();
                ModelState.AddModelError("", "Error al guardar el registro.");
                return View(model);
            }
        }

        // GET: Beneficiarios
        public async Task<IActionResult> Index()
        {
            var beneficiarios = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .OrderByDescending(b => b.Id_Beneficiario)  // ← CORREGIDO: Ordenar por ID
                .ToListAsync();

            return View(beneficiarios);
        }

        private async Task CargarEntidadesViewBag()
        {
            try
            {
                var entidades = await _context.Entidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre ?? "Sin nombre"
                    })
                    .ToListAsync();

                ViewBag.Entidades = entidades;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar entidades");
                ViewBag.Entidades = new List<SelectListItem>();
            }
        }
    }
}