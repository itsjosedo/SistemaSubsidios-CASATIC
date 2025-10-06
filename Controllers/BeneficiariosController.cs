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

        // GET: Beneficiarios
        public async Task<IActionResult> Index()
        {
            var beneficiarios = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .OrderByDescending(b => b.Id_Beneficiario)
                .ToListAsync();

            return View(beneficiarios);
        }

        // GET: Beneficiarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .FirstOrDefaultAsync(m => m.Id_Beneficiario == id);

            if (beneficiario == null)
            {
                return NotFound();
            }

            return View(beneficiario);
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
                    EstadoSubsidio = "Pendiente" // Estado por defecto
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

        // GET: Beneficiarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beneficiario = await _context.Beneficiarios.FindAsync(id);
            if (beneficiario == null)
            {
                return NotFound();
            }

            await CargarEntidadesViewBag();
            
            // Crear ViewModel para edición
            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Telefono = beneficiario.Telefono,
                Direccion = beneficiario.Direccion,
                EntidadId = beneficiario.EntidadId,
                EstadoSubsidio = beneficiario.EstadoSubsidio
            };

            return View(model);
        }

        // POST: Beneficiarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BeneficiarioViewModel model)
        {
            if (id != model.Id_Beneficiario)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await CargarEntidadesViewBag();
                return View(model);
            }

            try
            {
                // Validar DUI único (excluyendo el actual)
                if (await _context.Beneficiarios.AnyAsync(b => b.Dui == model.Dui && b.Id_Beneficiario != id))
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

                var beneficiario = await _context.Beneficiarios.FindAsync(id);
                if (beneficiario == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades
                beneficiario.Nombre = model.Nombre.Trim();
                beneficiario.Dui = model.Dui;
                beneficiario.Telefono = model.Telefono;
                beneficiario.Direccion = model.Direccion.Trim();
                beneficiario.EntidadId = model.EntidadId;
                beneficiario.EstadoSubsidio = model.EstadoSubsidio;

                _context.Beneficiarios.Update(beneficiario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Beneficiario actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeneficiarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar beneficiario");
                await CargarEntidadesViewBag();
                ModelState.AddModelError("", "Error al actualizar el registro.");
                return View(model);
            }
        }

        // GET: Beneficiarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .FirstOrDefaultAsync(m => m.Id_Beneficiario == id);

            if (beneficiario == null)
            {
                return NotFound();
            }

            return View(beneficiario);
        }

        // POST: Beneficiarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var beneficiario = await _context.Beneficiarios.FindAsync(id);
                if (beneficiario != null)
                {
                    _context.Beneficiarios.Remove(beneficiario);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Beneficiario eliminado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró el beneficiario a eliminar";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar beneficiario");
                TempData["ErrorMessage"] = "Error al eliminar el beneficiario";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BeneficiarioExists(int id)
        {
            return _context.Beneficiarios.Any(e => e.Id_Beneficiario == id);
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
                ViewBag.HayEntidades = entidades.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar entidades");
                ViewBag.Entidades = new List<SelectListItem>();
                ViewBag.HayEntidades = false;
            }
        }
    }
}