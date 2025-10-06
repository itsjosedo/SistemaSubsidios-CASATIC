using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class EntidadController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EntidadController> _logger;

        public EntidadController(AppDbContext db, ILogger<EntidadController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: Entidad
        public async Task<IActionResult> Index()
        {
            var entidades = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
            return View(entidades);
        }

        // GET: Entidad/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entidad == null)
                return NotFound();

            return View(entidad);
        }

        // GET: Entidad/Create
        public IActionResult CreateEntidad()
        {
            return View();
        }

        // POST: Entidad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntidad(EntidadViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (await _db.Entidades.AnyAsync(e => e.Nombre == model.Nombre))
                {
                    ModelState.AddModelError("Nombre", "Ya existe una entidad con este nombre.");
                    return View(model);
                }

                var entidad = new Entidad
                {
                    Nombre = model.Nombre,
                    Email = model.Email,
                    Direccion = model.Direccion
                };

                await _db.Entidades.AddAsync(entidad);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Entidad creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la entidad");
                ModelState.AddModelError("", $"Ocurrió un error: {ex.Message}");
                return View(model);
            }
        }

        // GET: Entidad/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entidad = await _db.Entidades.FindAsync(id);
            if (entidad == null)
                return NotFound();

            var model = new EntidadViewModel
            {
                Nombre = entidad.Nombre,
                Email = entidad.Email,
                Direccion = entidad.Direccion
            };

            return View(model);
        }

        // POST: Entidad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EntidadViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var entidad = await _db.Entidades.FindAsync(id);
                if (entidad == null)
                    return NotFound();

                if (await _db.Entidades.AnyAsync(e => e.Nombre == model.Nombre && e.Id != id))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otra entidad con este nombre.");
                    return View(model);
                }

                entidad.Nombre = model.Nombre;
                entidad.Email = model.Email;
                entidad.Direccion = model.Direccion;

                _db.Update(entidad);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Entidad actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar la entidad");
                ModelState.AddModelError("", "Ocurrió un error al actualizar la entidad.");
                return View(model);
            }
        }

        // GET: Entidad/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (entidad == null)
                return NotFound();

            if (entidad.Beneficiarios != null && entidad.Beneficiarios.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar la entidad porque tiene beneficiarios asociados.";
                return RedirectToAction(nameof(Index));
            }

            return View(entidad);
        }

        // POST: Entidad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var entidad = await _db.Entidades
                    .Include(e => e.Beneficiarios)
                    .FirstOrDefaultAsync(e => e.Id == id);
                    
                if (entidad == null)
                    return NotFound();

                if (entidad.Beneficiarios != null && entidad.Beneficiarios.Any())
                {
                    TempData["ErrorMessage"] = "No se puede eliminar la entidad porque tiene beneficiarios asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _db.Entidades.Remove(entidad);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Entidad eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la entidad");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la entidad.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =============================================
        // MÉTODOS PARA ASIGNAR BENEFICIARIOS (CORREGIDOS)
        // =============================================

        // GET: Entidad/GestionarBeneficiarios/5
        public async Task<IActionResult> GestionarBeneficiarios(int id)
        {
            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (entidad == null)
                return NotFound();

            // Obtener beneficiarios disponibles (sin entidad asignada)
            var beneficiariosDisponibles = await _db.Beneficiarios
                .Where(b => b.EntidadId == null)
                .ToListAsync();

            ViewBag.BeneficiariosDisponibles = beneficiariosDisponibles;

            // CAMBIO: Enviamos la entidad directamente, NO un ViewModel
            return View(entidad);
        }

        // POST: Entidad/AsignarBeneficiario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarBeneficiario(int entidadId, int beneficiarioId)
        {
            try
            {
                var beneficiario = await _db.Beneficiarios.FindAsync(beneficiarioId);
                if (beneficiario == null)
                {
                    TempData["ErrorMessage"] = "Beneficiario no encontrado.";
                    return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
                }

                if (beneficiario.EntidadId != null)
                {
                    TempData["ErrorMessage"] = "Este beneficiario ya está asignado a otra entidad.";
                    return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
                }

                beneficiario.EntidadId = entidadId;
                _db.Beneficiarios.Update(beneficiario);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Beneficiario asignado correctamente.";
                return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar beneficiario");
                TempData["ErrorMessage"] = "Error al asignar el beneficiario.";
                return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
            }
        }

        // POST: Entidad/DesasignarBeneficiario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasignarBeneficiario(int id, int entidadId)
        {
            try
            {
                var beneficiario = await _db.Beneficiarios.FindAsync(id);
                if (beneficiario == null)
                {
                    TempData["ErrorMessage"] = "Beneficiario no encontrado.";
                    return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
                }

                if (beneficiario.EntidadId != entidadId)
                {
                    TempData["ErrorMessage"] = "Este beneficiario no pertenece a esta entidad.";
                    return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
                }

                beneficiario.EntidadId = null;
                _db.Beneficiarios.Update(beneficiario);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Beneficiario desasignado correctamente.";
                return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desasignar beneficiario");
                TempData["ErrorMessage"] = "Error al desasignar el beneficiario.";
                return RedirectToAction("GestionarBeneficiarios", new { id = entidadId });
            }
        }
    }
}