using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class SubsidiosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubsidiosController> _logger;

        public SubsidiosController(AppDbContext context, ILogger<SubsidiosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Subsidios
        public async Task<IActionResult> Index()
        {
            var subsidios = await _context.Subsidios
                .Include(s => s.Beneficiario)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return View(subsidios);
        }

        // GET: Subsidios/Create
        public async Task<IActionResult> Create()
        {
            await CargarBeneficiarios();
            return View();
        }

        // POST: Subsidios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subsidio model)
        {
            if (!ModelState.IsValid)
            {
                await CargarBeneficiarios();
                return View(model);
            }

            try
            {
                _context.Subsidios.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subsidio creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear subsidio");
                ModelState.AddModelError("", "Ocurrió un error al guardar el subsidio");
                await CargarBeneficiarios();
                return View(model);
            }
        }

        // GET: Subsidios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var subsidio = await _context.Subsidios.FindAsync(id);
            if (subsidio == null) return NotFound();

            await CargarBeneficiarios();
            return View(subsidio);
        }

        // POST: Subsidios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subsidio model)
        {
            if (!ModelState.IsValid)
            {
                await CargarBeneficiarios();
                return View(model);
            }

            try
            {
                _context.Subsidios.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subsidio actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar subsidio");
                ModelState.AddModelError("", "Ocurrió un error al actualizar el subsidio");
                await CargarBeneficiarios();
                return View(model);
            }
        }

        // GET: Subsidios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();
            return View(subsidio);
        }

        // GET: Subsidios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();
            return View(subsidio);
        }

        // POST: Subsidios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subsidio = await _context.Subsidios.FindAsync(id);
            if (subsidio != null)
            {
                _context.Subsidios.Remove(subsidio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Método privado para llenar SelectList de Beneficiarios
        private async Task CargarBeneficiarios()
        {
            var beneficiarios = await _context.Beneficiarios
                .OrderBy(b => b.Nombre)
                .Select(b => new SelectListItem
                {
                    Value = b.Id_Beneficiario.ToString(),
                    Text = b.Nombre
                })
                .ToListAsync();

            ViewBag.Beneficiarios = beneficiarios;
        }
    }
}
