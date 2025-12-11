using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class AdminController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarEntidadesViewBag();
            return View();
        }

        //Metodo para crear un nuevo beneficiario
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

                // Validacion si existe un dui o no
                if (await _context.Beneficiarios.AnyAsync(b => b.Dui == model.Dui))
                {
                    ModelState.AddModelError("Dui", "Este DUI ya está registrado en el sistema");
                    await CargarEntidadesViewBag();
                    return View(model);
                }

                // Validar que la entidad existe
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

                //Creacion de un  nuevo beneficiario
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
            _logger.LogInformation("=== EDIT GET INICIADO === | ID recibido: {Id}", id);

            if (id == null)
            {
                _logger.LogWarning("ID es nulo");
                return NotFound();
            }

            var beneficiario = await _context.Beneficiarios.FindAsync(id);
            if (beneficiario == null)
                {
                 _logger.LogWarning("Beneficiario con ID {Id} no encontrado", id);
                return NotFound();
                }

    await CargarEntidadesViewBag();

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

    _logger.LogInformation("Edit GET cargado correctamente: {Nombre} (ID {Id})", 
        model.Nombre, model.Id_Beneficiario);

    return View(model);
}


        // POST: Beneficiarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BeneficiarioViewModel model)
        {
            _logger.LogInformation("=== EDIT POST INICIADO === | Modelo ID: {Id}", model.Id_Beneficiario);
             ModelState.Remove(nameof(model.Genero));
                // Eliminar validación obligatoria de AceptaTerminos en edición
                if (ModelState.ContainsKey("AceptaTerminos"))
                {
                    ModelState["AceptaTerminos"].Errors.Clear();
                    _logger.LogInformation("Error de AceptaTerminos eliminado para edición");
                }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("=== MODELSTATE NO VÁLIDO ===");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError("Campo: {Campo} | Error: {Error}", state.Key, error.ErrorMessage);
                    }
                }

            await CargarEntidadesViewBag();
            return View(model);
        }

            try
             {
            var beneficiario = await _context.Beneficiarios.FindAsync(model.Id_Beneficiario);

            if (beneficiario == null)
            {
                _logger.LogError("Beneficiario con ID {Id} no existe en la BD", model.Id_Beneficiario);
                return NotFound();
            }

            // Validar DUI único
            bool duiDuplicado = await _context.Beneficiarios
                .AnyAsync(b => b.Dui == model.Dui && b.Id_Beneficiario != model.Id_Beneficiario);

            if (duiDuplicado)
            {
                _logger.LogWarning("Intento de actualizar con DUI duplicado: {Dui}", model.Dui);
                ModelState.AddModelError("Dui", "Este DUI ya está registrado en el sistema");
                await CargarEntidadesViewBag();
                return View(model);
            }

            // Validar entidad existente
            if (model.EntidadId.HasValue)
            {
                bool entidadExiste = await _context.Entidades
                    .AnyAsync(e => e.Id == model.EntidadId.Value);

                if (!entidadExiste)
                {
                    _logger.LogWarning("Entidad seleccionada no válida: {Entidad}", model.EntidadId);
                    ModelState.AddModelError("EntidadId", "La entidad seleccionada no es válida");
                    await CargarEntidadesViewBag();
                    return View(model);
                }
            }
            ModelState.Remove(nameof(model.Genero));
            // Actualizar propiedades
            beneficiario.Nombre = model.Nombre?.Trim();
            beneficiario.Dui = model.Dui;
            beneficiario.Telefono = model.Telefono;
            beneficiario.Direccion = model.Direccion?.Trim();
            beneficiario.EntidadId = model.EntidadId;
            beneficiario.EstadoSubsidio = model.EstadoSubsidio;


            _context.Beneficiarios.Update(beneficiario);

            int cambios = await _context.SaveChangesAsync();
            _logger.LogInformation("=== ACTUALIZACIÓN EXITOSA === | Cambios guardados: {Cambios}", cambios);

            TempData["SuccessMessage"] = "Los datos del beneficiario se han actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al editar beneficiario");
            ModelState.AddModelError("", "Ocurrió un error al actualizar los datos del beneficiario.");

            await CargarEntidadesViewBag();
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
                _logger.LogInformation($"Cargadas {entidades.Count} entidades en ViewBag");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar entidades");
                ViewBag.Entidades = new List<SelectListItem>();
            }
        }
    }
}