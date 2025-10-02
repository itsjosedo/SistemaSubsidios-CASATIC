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
            var entidades = await _db.Entidades.OrderBy(e => e.Nombre).ToListAsync();
            return View(entidades);
        }

        // GET: Entidad/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entidad = await _db.Entidades.FirstOrDefaultAsync(e => e.Id == id);
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
                // Validar nombre único
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
                //_logger.LogError(ex, "Error al crear la entidad");
                //ModelState.AddModelError("", $"Ocurrió un error: {ex.Message}");
                //ModelState.AddModelError("", "Ocurrió un error al guardar la entidad.");
                _logger.LogError(ex, "Error al crear la entidad");

                if (ex.InnerException != null)
                    ModelState.AddModelError("", $"Error interno: {ex.InnerException.Message}");
                else
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

                // Validar nombre único excluyendo la entidad actual
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
            var entidad = await _db.Entidades.FindAsync(id);
            if (entidad == null)
                return NotFound();

            return View(entidad);
        }

        // POST: Entidad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var entidad = await _db.Entidades.FindAsync(id);
                if (entidad == null)
                    return NotFound();

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
    }
}
