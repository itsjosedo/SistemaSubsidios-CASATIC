using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaSubsidios_CASATIC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            IQueryable<Subsidio> subsidiosQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => !s.Eliminado) // 🔹 FILTRO: Solo subsidios NO eliminados
                .OrderByDescending(s => s.Id);

            // Filtro para Entidades: Solo ven lo que ellos crearon
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                subsidiosQuery = subsidiosQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var subsidios = await subsidiosQuery.ToListAsync();

            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.TieneSubsidios = subsidios.Any();

            return View(subsidios);
        }

        // GET: Subsidios/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                var tieneSubsidios = _context.Subsidios.Any(s => s.UsuarioCreacionId == userId.Value.ToString() && !s.Eliminado);
                ViewBag.TieneSubsidios = tieneSubsidios;
                ViewBag.UserId = userId.Value.ToString();
            }
            else
            {
                ViewBag.TieneSubsidios = true;
                ViewBag.UserId = "0";
            }

            return View();
        }

        // POST: Subsidios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subsidio model)
        {
            _logger.LogInformation("🎯 === INICIANDO CREACIÓN DE SUBSIDIO ===");

            // Remover validación de propiedades
            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");
            ModelState.Remove("UsuarioCreacionId");

            if (!ModelState.IsValid)
            {
                _logger.LogError("❌ MODELSTATE NO VÁLIDO");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // Asignación UsuarioCreacionId
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    model.UsuarioCreacionId = userId.Value.ToString();
                }
                else
                {
                    model.UsuarioCreacionId = "0";
                }

                // 🔹 Inicializar propiedades de Soft Delete
                model.Eliminado = false;
                model.FechaEliminacion = null;
                model.UsuarioEliminacionId = null;
                model.MotivoEliminacion = null;

                _context.Subsidios.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' creado correctamente. Ahora puede asignar beneficiarios.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear subsidio");
                ModelState.AddModelError("", "Error al crear el subsidio: " + ex.Message);
                return View(model);
            }
        }

        // GET: Subsidios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado); // 🔹 Solo si no está eliminado

            if (subsidio == null) return NotFound();

            // Verificar permisos para usuarios/Entidad
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            return View(subsidio);
        }

        // POST: Subsidios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subsidio model)
        {
            ModelState.Remove("UsuarioCreacionId");
            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidioExistente = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == model.Id && !s.Eliminado);

                if (subsidioExistente == null)
                {
                    ModelState.AddModelError("", "El subsidio que intenta actualizar no existe");
                    return View(model);
                }

                // Verificar permisos
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidioExistente.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para editar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                subsidioExistente.NombrePrograma = model.NombrePrograma;
                subsidioExistente.Tipo = model.Tipo;
                subsidioExistente.Monto = model.Monto;
                subsidioExistente.FechaAsignacion = model.FechaAsignacion;
                subsidioExistente.Estado = model.Estado;
                subsidioExistente.FechaExpiracion = model.FechaExpiracion;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar subsidio");
                ModelState.AddModelError("", "Error al actualizar el subsidio: " + ex.Message);
                return View(model);
            }
        }
        //Metodo para mostar los detalles de cada subsdio asigano al beneficiario
        public async Task<IActionResult> DetallesSubsidios(int id)
        {
            var subsidio = await _context.Subsidios
            .Include(s => s.Beneficiarios) // ✔ Sí está mapeado
            .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null)
                return NotFound();

            // Obtener el usuario creador
            var usuarioCreador = await _context.Usuarios
                .Include(u => u.Entidad)
                .FirstOrDefaultAsync(u => u.Id_Usuario.ToString() == subsidio.UsuarioCreacionId);

            ViewBag.EntidadCreadora = usuarioCreador?.Entidad?.Nombre ?? "No asignada";

            return View(subsidio);
        }
        // GET: Subsidios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            // 🔹 QUITAR EL FILTRO de Eliminado
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);  // Sin .Where(s => !s.Eliminado)

            if (subsidio == null) return NotFound();

            // Verificar permisos
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para ver este subsidio";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Pasar TODA la información sobre eliminación
            ViewBag.EstaEliminado = subsidio.Eliminado;

            // 🔹 CRÍTICO: Pasar las propiedades específicas de eliminación
            if (subsidio.Eliminado)
            {
                ViewBag.MotivoEliminacion = subsidio.MotivoEliminacion;
                ViewBag.FechaEliminacion = subsidio.FechaEliminacion;
                ViewBag.UsuarioEliminacionId = subsidio.UsuarioEliminacionId;
            }

            return View(subsidio);
        }

        // GET: Subsidios/Renovar/5
        public async Task<IActionResult> Renovar(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado); // 🔹 Solo si no está eliminado

            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Verificar permisos
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para renovar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que el subsidio esté activo
            if (subsidio.Estado != "Activo")
            {
                TempData["WarningMessage"] = "Solo se pueden renovar subsidios activos";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Calcular fecha sugerida de expiración
            ViewBag.FechaSugeridaExpiracion = CalcularFechaSugeridaRenovacion(subsidio);

            return View(subsidio);
        }

        // POST: Subsidios/Renovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renovar(int id, DateTime nuevaFechaExpiracion)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();

                var subsidio = await _context.Subsidios
                    .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado); // 🔹 Solo si no está eliminado

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para renovar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar que el subsidio esté activo
                if (subsidio.Estado != "Activo")
                {
                    TempData["WarningMessage"] = "Solo se pueden renovar subsidios activos";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Validar fecha de expiración
                if (nuevaFechaExpiracion <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "La fecha de expiración debe ser futura";
                    return RedirectToAction(nameof(Renovar), new { id });
                }

                // Guardar la fecha de renovación actual
                subsidio.FechaRenovacion = DateTime.Now;
                subsidio.FechaExpiracion = nuevaFechaExpiracion;
                subsidio.Estado = "Activo"; // Mantener activo

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{subsidio.NombrePrograma}' renovado exitosamente hasta el {nuevaFechaExpiracion:dd/MM/yyyy}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar subsidio");
                TempData["ErrorMessage"] = "Error al renovar el subsidio: " + ex.Message;
                return RedirectToAction(nameof(Renovar), new { id });
            }
        }

        // GET: Subsidios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado); // 🔹 Solo si no está eliminado

            if (subsidio == null) return NotFound();

            // Verificar permisos
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            return View(subsidio);
        }

        // POST: Subsidios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? motivoEliminacion = null)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para eliminar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                // 🔹 BORRADO LÓGICO en lugar de físico
                subsidio.Eliminado = true;
                subsidio.FechaEliminacion = DateTime.Now;
                subsidio.UsuarioEliminacionId = userId.HasValue ? userId.Value.ToString() : "Sistema";
                subsidio.MotivoEliminacion = motivoEliminacion ?? "Eliminación por usuario";
                subsidio.Estado = "Inactivo"; // Cambiar estado a inactivo

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{subsidio.NombrePrograma}' eliminado correctamente (queda en historial)";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar (soft delete) subsidio");
                TempData["ErrorMessage"] = "Error al eliminar el subsidio: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Subsidios/Eliminados
        [Authorize(Roles = "Administrador,admin")]
        public async Task<IActionResult> Eliminados()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            IQueryable<Subsidio> subsidiosQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => s.Eliminado)
                .OrderByDescending(s => s.FechaEliminacion);

            // Filtro para Entidades: Solo ven lo que ellos crearon
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                subsidiosQuery = subsidiosQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var subsidiosEliminados = await subsidiosQuery.ToListAsync();

            ViewData["Title"] = "Subsidios Eliminados";
            ViewBag.TotalEliminados = subsidiosEliminados.Count;
            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";

            return View(subsidiosEliminados);
        }

        // POST: Subsidios/Restaurar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,admin")]
        public async Task<IActionResult> Restaurar(int id)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();

                var subsidio = await _context.Subsidios.FindAsync(id);
                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Eliminados));
                }

                // Verificar que esté eliminado
                if (!subsidio.Eliminado)
                {
                    TempData["WarningMessage"] = "Este subsidio no está eliminado";
                    return RedirectToAction(nameof(Eliminados));
                }

                // Verificar permisos para administradores
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para restaurar este subsidio";
                    return RedirectToAction(nameof(Eliminados));
                }

                // 🔹 RESTAURAR SUBSIDIO
                subsidio.Eliminado = false;
                subsidio.FechaEliminacion = null;
                subsidio.UsuarioEliminacionId = null;
                subsidio.MotivoEliminacion = null;
                subsidio.Estado = "Activo"; // Reactivar el subsidio

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{subsidio.NombrePrograma}' restaurado correctamente";
                return RedirectToAction(nameof(Eliminados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar subsidio");
                TempData["ErrorMessage"] = "Error al restaurar el subsidio: " + ex.Message;
                return RedirectToAction(nameof(Eliminados));
            }
        }

        // POST: Subsidios/EliminarPermanente/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,admin")]
        public async Task<IActionResult> EliminarPermanente(int id)
        {
            try
            {
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Eliminados));
                }

                if (!subsidio.Eliminado)
                {
                    TempData["WarningMessage"] = "Solo se pueden eliminar permanentemente subsidios marcados como eliminados";
                    return RedirectToAction(nameof(Eliminados));
                }

                var nombreSubsidio = subsidio.NombrePrograma;

                // 🔹 ELIMINACIÓN FÍSICA (solo para administradores)
                subsidio.Beneficiarios.Clear();
                _context.Subsidios.Remove(subsidio);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{nombreSubsidio}' eliminado permanentemente del sistema";
                return RedirectToAction(nameof(Eliminados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permanentemente subsidio");
                TempData["ErrorMessage"] = "Error al eliminar permanentemente: " + ex.Message;
                return RedirectToAction(nameof(Eliminados));
            }
        }

        // POST: Subsidios/LimpiarHistorialAntiguo
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,admin")]
        public async Task<IActionResult> LimpiarHistorialAntiguo()
        {
            try
            {
                var fechaLimite = DateTime.Now.AddYears(-1); // Más de 1 año

                var subsidiosAntiguos = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .Where(s => s.Eliminado &&
                           s.FechaEliminacion.HasValue &&
                           s.FechaEliminacion.Value < fechaLimite)
                    .ToListAsync();

                int eliminadosCount = 0;

                foreach (var subsidio in subsidiosAntiguos)
                {
                    subsidio.Beneficiarios.Clear();
                    _context.Subsidios.Remove(subsidio);
                    eliminadosCount++;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Se eliminaron permanentemente {eliminadosCount} subsidios del historial (más de 1 año)";
                return RedirectToAction(nameof(Eliminados));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar historial antiguo");
                TempData["ErrorMessage"] = "Error al limpiar historial: " + ex.Message;
                return RedirectToAction(nameof(Eliminados));
            }
        }

        // GET: Subsidios/Reporte
        public async Task<IActionResult> Reporte()
        {
            var subsidios = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => !s.Eliminado) // 🔹 Solo subsidios NO eliminados
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Reporte General de Subsidios";
            ViewData["FechaReporte"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewData["TotalSubsidios"] = subsidios.Count;
            ViewData["MontoTotal"] = subsidios.Sum(s => s.Monto * s.Beneficiarios.Count).ToString("N2");

            return View(subsidios);
        }

        // GET: Subsidios/Activos
        public async Task<IActionResult> Activos()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para ver los subsidios activos";
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Subsidio> subsidiosQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => s.Estado == "Activo" && !s.Eliminado); // 🔹 Solo activos y NO eliminados

            // Filtro para Entidades
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                subsidiosQuery = subsidiosQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var subsidiosActivos = await subsidiosQuery
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Subsidios Activos de Mi Entidad";
            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";
            return View(subsidiosActivos);
        }

        // Mostrar detalles del subsidio en beneficiario
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .ThenInclude(b => b.Entidad)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado && s.Beneficiarios.Any(b => b.UsuarioId == userId.Value));

            if (subsidio == null)
                return NotFound();

            return View(subsidio);
        }

        // MÉTODOS PARA ASIGNACIÓN MÚLTIPLE DE BENEFICIARIOS

        // GET: Subsidios/GestionarBeneficiarios/5
        public async Task<IActionResult> GestionarBeneficiarios(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado); // 🔹 Solo si no está eliminado

            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Verificar permisos
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                return RedirectToAction(nameof(Index));
            }

            // Obtener todos los beneficiarios disponibles
            var todosBeneficiarios = await _context.Beneficiarios
                .Where(b => !string.IsNullOrEmpty(b.Dui) && !string.IsNullOrEmpty(b.Nombre))
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewBag.BeneficiariosDisponibles = todosBeneficiarios;
            ViewBag.BeneficiariosAsignados = subsidio.Beneficiarios.Select(b => b.Id_Beneficiario).ToList();

            return View(subsidio);
        }

        // POST: Subsidios/GestionarBeneficiarios/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarBeneficiarios(int id, List<int> beneficiariosIds)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                subsidio.Beneficiarios.Clear();

                if (beneficiariosIds != null && beneficiariosIds.Any())
                {
                    var beneficiarios = await _context.Beneficiarios
                        .Where(b => beneficiariosIds.Contains(b.Id_Beneficiario))
                        .ToListAsync();

                    foreach (var beneficiario in beneficiarios)
                    {
                        subsidio.Beneficiarios.Add(beneficiario);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Beneficiarios actualizados correctamente para el subsidio '{subsidio.NombrePrograma}'";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gestionar beneficiarios");
                TempData["ErrorMessage"] = "Error al gestionar los beneficiarios: " + ex.Message;
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
        }

        // GET: Subsidios/QuitarBeneficiario/5
        public async Task<IActionResult> QuitarBeneficiario(int id, int beneficiarioId)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);

            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Verificar permisos
            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                return RedirectToAction(nameof(Index));
            }

            var beneficiario = subsidio.Beneficiarios.FirstOrDefault(b => b.Id_Beneficiario == beneficiarioId);
            if (beneficiario == null)
            {
                TempData["WarningMessage"] = "Beneficiario no encontrado en este subsidio";
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }

            ViewBag.Beneficiario = beneficiario;
            return View(subsidio);
        }

        // POST: Subsidios/QuitarBeneficiario/5
        [HttpPost, ActionName("QuitarBeneficiario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarBeneficiarioConfirmed(int id, int beneficiarioId)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                var beneficiario = subsidio.Beneficiarios.FirstOrDefault(b => b.Id_Beneficiario == beneficiarioId);
                if (beneficiario != null)
                {
                    subsidio.Beneficiarios.Remove(beneficiario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Beneficiario '{beneficiario.Nombre}' removido correctamente del subsidio '{subsidio.NombrePrograma}'";
                }

                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar beneficiario");
                TempData["ErrorMessage"] = "Error al quitar el beneficiario: " + ex.Message;
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
        }

        // MÉTODO PARA ASIGNAR SUBSIDIOS EXISTENTES
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignarSubsidiosExistente()
        {
            string usuarioCasaticId = "3";

            var subsidiosExistentes = await _context.Subsidios
                .Where(s => !s.Eliminado) // 🔹 Solo si no están eliminados
                .ToListAsync();

            int contador = 0;
            foreach (var subsidio in subsidiosExistentes)
            {
                if (string.IsNullOrEmpty(subsidio.UsuarioCreacionId))
                {
                    subsidio.UsuarioCreacionId = usuarioCasaticId;
                    contador++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Se asignaron {contador} subsidios existentes al usuario 'Casatic'";
            return RedirectToAction(nameof(Index));
        }

        // MÉTODOS AUXILIARES
        public JsonResult GetEstadisticasBeneficiarios()
        {
            var estadisticas = new
            {
                TotalSubsidios = _context.Subsidios.Count(s => !s.Eliminado),
                SubsidiosConBeneficiarios = _context.Subsidios.Count(s => s.Beneficiarios.Any() && !s.Eliminado),
                TotalBeneficiariosAsignados = _context.Subsidios.Where(s => !s.Eliminado).Sum(s => s.Beneficiarios.Count)
            };

            return Json(estadisticas);
        }

        // Método auxiliar para obtener fecha sugerida de renovación
        private DateTime CalcularFechaSugeridaRenovacion(Subsidio subsidio)
        {
            // Si ya ha sido renovado antes, usar el mismo período
            if (subsidio.FechaRenovacion.HasValue)
            {
                var ultimaDuracion = (subsidio.FechaExpiracion - subsidio.FechaRenovacion.Value).Days;
                return DateTime.Now.AddDays(ultimaDuracion);
            }

            // Si no, usar 1 año por defecto
            return DateTime.Now.AddYears(1);
        }
    }
}