using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class SubsidiosController : BaseController
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
            _logger.LogInformation("🎯 === INICIANDO CREACIÓN DE SUBSIDIO ===");
            
            // Log de todos los datos recibidos
            _logger.LogInformation("📥 DATOS RECIBIDOS:");
            _logger.LogInformation($"   Programa: {model.NombrePrograma ?? "NULL"}");
            _logger.LogInformation($"   Tipo: {model.Tipo ?? "NULL"}");
            _logger.LogInformation($"   Monto: {model.Monto}");
            _logger.LogInformation($"   BeneficiarioId: {model.BeneficiarioId}");
            _logger.LogInformation($"   FechaAsignacion: {model.FechaAsignacion}");
            _logger.LogInformation($"   Estado: {model.Estado ?? "NULL"}");

            // Validar ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogError("❌ MODELSTATE NO VÁLIDO - Errores encontrados:");
                
                foreach (var state in ModelState)
                {
                    var errors = state.Value.Errors;
                    if (errors.Count > 0)
                    {
                        _logger.LogError($"   📍 {state.Key}:");
                        foreach (var error in errors)
                        {
                            _logger.LogError($"      - {error.ErrorMessage}");
                        }
                    }
                }
                
                await CargarBeneficiarios();
                return View(model);
            }

            _logger.LogInformation("✅ MODELSTATE VÁLIDO - Todos los campos son correctos");

            try
            {
                _logger.LogInformation("🔍 Validando que el beneficiario existe...");
                
                // Validar que el beneficiario existe
                var beneficiarioExiste = await _context.Beneficiarios
                    .AnyAsync(b => b.Id_Beneficiario == model.BeneficiarioId);
                    
                if (!beneficiarioExiste)
                {
                    _logger.LogError($"❌ BENEFICIARIO NO ENCONTRADO - ID: {model.BeneficiarioId}");
                    ModelState.AddModelError("BeneficiarioId", "El beneficiario seleccionado no existe");
                    await CargarBeneficiarios();
                    return View(model);
                }

                _logger.LogInformation("✅ Beneficiario validado correctamente");

                // Proceder a guardar
                _logger.LogInformation("💾 Agregando subsidio al contexto de base de datos...");
                _context.Subsidios.Add(model);
                
                _logger.LogInformation("💿 Ejecutando SaveChangesAsync()...");
                int filasAfectadas = await _context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ SUBSIDIO GUARDADO EXITOSAMENTE");
                _logger.LogInformation($"   📋 ID generado: {model.Id}");
                _logger.LogInformation($"   📊 Filas afectadas: {filasAfectadas}");
                _logger.LogInformation($"   🏷️ Programa: {model.NombrePrograma}");

                // Configurar mensaje de éxito
                TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' creado correctamente";
                _logger.LogInformation($"📢 Mensaje de éxito configurado en TempData");

                _logger.LogInformation("🔄 Redirigiendo a Index...");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "💥 ERROR CRÍTICO DE BASE DE DATOS");
                _logger.LogError($"   Mensaje: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    _logger.LogError($"   Inner Exception: {dbEx.InnerException.Message}");
                }
                
                ModelState.AddModelError("", "Error de base de datos al guardar el subsidio. Verifique los datos.");
                await CargarBeneficiarios();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR INESPERADO");
                _logger.LogError($"   Tipo: {ex.GetType().Name}");
                _logger.LogError($"   Mensaje: {ex.Message}");
                
                ModelState.AddModelError("", "Ocurrió un error inesperado al guardar el subsidio");
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
    _logger.LogInformation("🎯 === INICIANDO ACTUALIZACIÓN DE SUBSIDIO ===");
    _logger.LogInformation($"📝 ID del subsidio a actualizar: {model.Id}");
    
    // Log de todos los datos recibidos
    _logger.LogInformation("📥 DATOS RECIBIDOS PARA ACTUALIZAR:");
    _logger.LogInformation($"   Programa: {model.NombrePrograma ?? "NULL"}");
    _logger.LogInformation($"   Tipo: {model.Tipo ?? "NULL"}");
    _logger.LogInformation($"   Monto: {model.Monto}");
    _logger.LogInformation($"   BeneficiarioId: {model.BeneficiarioId}");
    _logger.LogInformation($"   FechaAsignacion: {model.FechaAsignacion}");
    _logger.LogInformation($"   Estado: {model.Estado ?? "NULL"}");

    // Validar ModelState
    if (!ModelState.IsValid)
    {
        _logger.LogError("❌ MODELSTATE NO VÁLIDO - Errores encontrados:");
        
        foreach (var state in ModelState)
        {
            var errors = state.Value.Errors;
            if (errors.Count > 0)
            {
                _logger.LogError($"   📍 {state.Key}:");
                foreach (var error in errors)
                {
                    _logger.LogError($"      - {error.ErrorMessage}");
                }
            }
        }
        
        await CargarBeneficiarios();
        return View(model);
    }

    _logger.LogInformation("✅ MODELSTATE VÁLIDO");

    try
    {
        _logger.LogInformation("🔍 Buscando subsidio existente en la base de datos...");
        
        // Buscar el subsidio existente
        var subsidioExistente = await _context.Subsidios.FindAsync(model.Id);
        if (subsidioExistente == null)
        {
            _logger.LogError($"❌ SUBSIDIO NO ENCONTRADO - ID: {model.Id}");
            ModelState.AddModelError("", "El subsidio que intenta actualizar no existe");
            await CargarBeneficiarios();
            return View(model);
        }

        _logger.LogInformation("✅ Subsidio encontrado, actualizando propiedades...");

        // Actualizar propiedades
        subsidioExistente.NombrePrograma = model.NombrePrograma;
        subsidioExistente.Tipo = model.Tipo;
        subsidioExistente.Monto = model.Monto;
        subsidioExistente.BeneficiarioId = model.BeneficiarioId;
        subsidioExistente.FechaAsignacion = model.FechaAsignacion;
        subsidioExistente.Estado = model.Estado;

        _logger.LogInformation("💾 Actualizando subsidio en el contexto...");
        _context.Subsidios.Update(subsidioExistente);
        
        _logger.LogInformation("💿 Ejecutando SaveChangesAsync()...");
        int filasAfectadas = await _context.SaveChangesAsync();
        
        _logger.LogInformation($"✅ SUBSIDIO ACTUALIZADO EXITOSAMENTE");
        _logger.LogInformation($"   📊 Filas afectadas: {filasAfectadas}");
        _logger.LogInformation($"   🏷️ Programa actualizado: {model.NombrePrograma}");

        // Configurar mensaje de éxito
        TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' actualizado correctamente";
        _logger.LogInformation($"📢 Mensaje de éxito configurado en TempData: {TempData["SuccessMessage"]}");

        _logger.LogInformation("🔄 Redirigiendo a Index...");
        return RedirectToAction(nameof(Index));
    }
    catch (DbUpdateException dbEx)
    {
        _logger.LogError(dbEx, "💥 ERROR DE BASE DE DATOS al actualizar subsidio");
        _logger.LogError($"   Mensaje: {dbEx.Message}");
        if (dbEx.InnerException != null)
        {
            _logger.LogError($"   Inner Exception: {dbEx.InnerException.Message}");
        }
        
        ModelState.AddModelError("", "Error de base de datos al actualizar el subsidio. Verifique los datos.");
        await CargarBeneficiarios();
        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR INESPERADO al actualizar subsidio");
        _logger.LogError($"   Tipo: {ex.GetType().Name}");
        _logger.LogError($"   Mensaje: {ex.Message}");
        
        ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar el subsidio");
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
        
// GET: Subsidios/Reporte
public async Task<IActionResult> Reporte()
{
    var subsidios = await _context.Subsidios
        .Include(s => s.Beneficiario)
        .OrderByDescending(s => s.Id)
        .ToListAsync();

    ViewData["Title"] = "Reporte General de Subsidios";
    ViewData["FechaReporte"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    ViewData["TotalSubsidios"] = subsidios.Count;
    ViewData["MontoTotal"] = subsidios.Sum(s => s.Monto).ToString("N2");
    
    return View(subsidios);
}

        public async Task<IActionResult> Activos()
{
    var subsidiosActivos = await _context.Subsidios
        .Include(s => s.Beneficiario)
        .Where(s => s.Estado == "Activo") // Ajusta según tu campo de estado
        .OrderByDescending(s => s.Id)
        .ToListAsync();

    ViewData["Title"] = "Subsidios Activos";
    return View(subsidiosActivos);
}

        // Método privado para llenar SelectList de Beneficiarios
        private async Task CargarBeneficiarios()
        {
            var beneficiarios = await _context.Beneficiarios
                .OrderBy(b => b.Dui)
                .Select(b => new SelectListItem
                {
                    Value = b.Id_Beneficiario.ToString(),
                    Text = b.Dui
                })
                .ToListAsync();

            ViewBag.Beneficiarios = beneficiarios;
        }
    }
}