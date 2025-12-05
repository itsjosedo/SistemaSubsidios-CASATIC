using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class EntidadController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EntidadController> _logger;

        public EntidadController(AppDbContext db, ILogger<EntidadController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var entidades = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
            return View(entidades);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entidad == null)
                return NotFound();

            return View(entidad);
        }

        [HttpGet]
        public IActionResult CreateEntidad()
        {
            return View();
        }

        
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
                if (await _db.Usuarios.AnyAsync(u => u.Correo == model.CorreoUsuario))
                {
                    ModelState.AddModelError("CorreoUsuario", "Ya existe un usuario con este correo.");
                    return View(model);
                }
                var usuario = new Usuario
                {
                    Nombre = model.NombreUsuario,
                    Correo = model.CorreoUsuario,
                    Contrasena = AuthHelper.Hash(model.Contrasena),
                    Rol = "entidad",
                    Estado = "activo"
                };

                await _db.Usuarios.AddAsync(usuario);
                await _db.SaveChangesAsync();

                var entidad = new Entidad
                {
                    Nombre = model.Nombre,
                    Email = model.Email,
                    Direccion = model.Direccion,
                    UsuarioId = usuario.Id_Usuario

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation($"Edit GET llamado - ID: {id}");
            
            var entidad = await _db.Entidades.FindAsync(id);
            if (entidad == null)
            {
                _logger.LogWarning($"Entidad con ID {id} no encontrada");
                return NotFound();
            }

            var model = new EntidadViewModel
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                Email = entidad.Email,
                Direccion = entidad.Direccion
            };

            _logger.LogInformation($"Cargando entidad: {model.Nombre} (ID: {model.Id})");
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EntidadViewModel model)
        {
            _logger.LogInformation($"Iniciando actualización para entidad ID: {model.Id}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido");
                return View(model);
            }

            try
            {
                var entidad = await _db.Entidades.FindAsync(model.Id);
                if (entidad == null)
                {
                    _logger.LogWarning($"Entidad con ID {model.Id} no encontrada");
                    return NotFound();
                }

                // Verificar duplicados
                bool existeDuplicado = await _db.Entidades
                    .AnyAsync(e => e.Nombre == model.Nombre && e.Id != model.Id);
                    
                if (existeDuplicado)
                {
                    _logger.LogWarning($"Nombre duplicado: {model.Nombre}");
                    ModelState.AddModelError("Nombre", "Ya existe otra entidad con este nombre.");
                    return View(model);
                }

                // Actualizar propiedades
                entidad.Nombre = model.Nombre;
                entidad.Email = model.Email;
                entidad.Direccion = model.Direccion;

                _db.Entidades.Update(entidad);
                int cambios = await _db.SaveChangesAsync();
                
                _logger.LogInformation($"Actualización completada. Cambios guardados: {cambios}");

                TempData["SuccessMessage"] = "Entidad actualizada correctamente.";
                return RedirectToAction("Details", new { id = entidad.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar entidad");
                ModelState.AddModelError("", "La entidad fue modificada por otro usuario. Por favor, recarga la página e intenta nuevamente.");
                return View(model);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al actualizar entidad");
                ModelState.AddModelError("", "Error de base de datos al actualizar la entidad.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al editar la entidad");
                ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar la entidad.");
                return View(model);
            }
        }

        //Muestra la página de confirmación
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET - ID es null");
                return NotFound();
            }

            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entidad == null)
            {
                _logger.LogWarning($"Delete GET - Entidad con ID {id} no encontrada");
                return NotFound();
            }

            return View(entidad);
        }

        //Ejecuta la eliminación
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
                    return RedirectToAction(nameof(Delete), new { id = id });
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

        
        // MÉTODOS PARA ASIGNAR BENEFICIARIOS 
        [HttpGet]
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
        
        // GET: Entidad/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Verificar que sea una entidad
            if (GetRolUsuario() != "entidad" && GetRolUsuario() != "operador")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var entidadId = GetEntidadId();
            if (!entidadId.HasValue)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Obtener estadísticas para el dashboard
            var entidad = await _db.Entidades
                .Include(e => e.Beneficiarios)
                .FirstOrDefaultAsync(e => e.Id == entidadId);

            if (entidad == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Obtener estadísticas
            var totalBeneficiarios = entidad.Beneficiarios?.Count ?? 0;
            var beneficiariosActivos = entidad.Beneficiarios?.Count(b => b.EstadoSubsidio?.ToLower() == "activo") ?? 0;
            var beneficiariosPendientes = entidad.Beneficiarios?.Count(b => b.EstadoSubsidio?.ToLower() == "pendiente") ?? 0;

            ViewBag.TotalBeneficiarios = totalBeneficiarios;
            ViewBag.BeneficiariosActivos = beneficiariosActivos;
            ViewBag.BeneficiariosPendientes = beneficiariosPendientes;
            ViewBag.NombreEntidad = entidad.Nombre;

            return View();
        }
    }
}