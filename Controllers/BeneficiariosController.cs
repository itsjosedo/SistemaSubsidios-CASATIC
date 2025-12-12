using SistemaSubsidios_CASATIC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Text; 
using System.ComponentModel.DataAnnotations; 

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BeneficiariosController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BeneficiariosController> _logger;
        private readonly LogService _log;

        public BeneficiariosController(AppDbContext context, ILogger<BeneficiariosController> logger, LogService log)
        {
            _context = context;
            _logger = logger;
            _log = log;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            ViewBag.UserId = userId;

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                ViewBag.PerfilIncompleto = true;
                return View(new BeneficiarioViewModel());
            }

            bool perfilIncompleto = string.IsNullOrWhiteSpace(beneficiario.Direccion)
              || string.IsNullOrWhiteSpace(beneficiario.Telefono)
              || string.IsNullOrWhiteSpace(beneficiario.Dui);

            ViewBag.PerfilIncompleto = perfilIncompleto;

            var subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>();
            ViewBag.Subsidios = subsidios;
            
            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Direccion = beneficiario.Direccion,
                Telefono = beneficiario.Telefono,
                EntidadId = beneficiario.EntidadId,
                EstadoSubsidio = beneficiario.EstadoSubsidio,
                EntidadNombre = beneficiario.Entidad?.Nombre,
                Genero = beneficiario.Genero,
                FechaNacimiento = beneficiario.FechaNacimiento,
                Subsidios = subsidios
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompletarPerfil()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                beneficiario = new Beneficiario { UsuarioId = userId.Value };
                _context.Beneficiarios.Add(beneficiario);
                await _context.SaveChangesAsync();
            }

            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Direccion = beneficiario.Direccion,
                Telefono = beneficiario.Telefono,
                Genero = beneficiario.Genero,
                FechaNacimiento = beneficiario.FechaNacimiento,
                Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CompletarPerfil(BeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Errores de validación: {Errores}", string.Join(", ", errores));
                ViewBag.Errores = errores;
                return View(model);
            }

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var beneficiario = await _context.Beneficiarios
                    .Include(b => b.Subsidios)
                    .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

                if (beneficiario == null)
                {
                    beneficiario = new Beneficiario { UsuarioId = userId.Value };
                    _context.Beneficiarios.Add(beneficiario);
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id_Usuario == userId.Value);
                if (usuario != null) beneficiario.Nombre = usuario.Nombre;

                beneficiario.Dui = model.Dui?.Trim();
                beneficiario.Telefono = model.Telefono?.Trim();
                beneficiario.Direccion = model.Direccion?.Trim();
                beneficiario.Genero = model.Genero?.Trim();
                beneficiario.FechaNacimiento = model.FechaNacimiento;

                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Tu perfil se ha completado correctamente.";
                return RedirectToAction("Index", "Beneficiarios");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar el perfil");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos.");
                return View(model);
            }
        }

        public async Task<IActionResult> Lista()
        {
            var beneficiarios = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .Include(b => b.Subsidios)
                .Where(b => !string.IsNullOrEmpty(b.Dui) && !string.IsNullOrEmpty(b.Nombre))
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewData["Title"] = "Lista de Beneficiarios";
            return View(beneficiarios);
        }

        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null) return RedirectToAction("CompletarPerfil");

            var model = new BeneficiarioViewModel
            {
                Telefono = beneficiario.Telefono,
                Direccion = beneficiario.Direccion,
                Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(BeneficiarioViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null) return RedirectToAction("CompletarPerfil");

            ModelState.Remove("Nombre");
            ModelState.Remove("Dui");
            ModelState.Remove("Correo");
            ModelState.Remove("Genero");
            ModelState.Remove("FechaNacimiento");

            if (!ModelState.IsValid)
            {
                model.Telefono = beneficiario.Telefono;
                model.Direccion = beneficiario.Direccion;
                model.Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>();
                return View(model);
            }

            beneficiario.Telefono = model.Telefono?.Trim();
            beneficiario.Direccion = model.Direccion?.Trim();
            
            await _log.Registrar(usuarioId: beneficiario.UsuarioId, accion: "Actualización de perfil",
            datos: $"Teléfono: {beneficiario.Telefono}, Dirección: {beneficiario.Direccion}"
);

            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // ==========================================
        // 📥 MÓDULO DE IMPORTACIÓN CSV (CON AUTO-FORMATO DUI)
        // ==========================================
        [HttpGet]
        public IActionResult CargaMasiva()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarCargaMasiva(IFormFile archivoCSV)
        {
            if (archivoCSV == null || archivoCSV.Length == 0)
            {
                ViewBag.Error = "Por favor, selecciona un archivo CSV válido.";
                return View("CargaMasiva");
            }

            var nuevosBeneficiarios = new List<Beneficiario>();
            var listaErrores = new List<string>();
            int filasProcesadas = 0;

            try
            {
                using (var stream = new StreamReader(archivoCSV.OpenReadStream()))
                using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                {
                    var registrosCsv = csv.GetRecords<BeneficiarioImportDto>().ToList();
                    
                    // Preparamos los DUIs para validación masiva (Evita N+1)
                    var duisEnCsv = registrosCsv
                        .Select(r => r.Dui?.Replace("-", "").Trim()) 
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToHashSet();

                    // Traemos DUIs existentes (buscamos tanto con guion como sin guion para asegurar)
                    var duisExistentesDb = await _context.Beneficiarios
                        .AsNoTracking()
                        .ToListAsync(); 

                    var setDuisExistentes = duisExistentesDb
                        .Select(b => b.Dui.Replace("-", "").Trim()) // Normalizamos lo de la BD
                        .ToHashSet();

                    var setDuisEnEsteArchivo = new HashSet<string>(); 

                    foreach (var fila in registrosCsv)
                    {
                        filasProcesadas++;

                        // =======================================================
                        // 🛠️ LOGICA DE FORMATEO AUTOMÁTICO (CORRECCIÓN DE DUI)
                        // =======================================================
                        if (!string.IsNullOrWhiteSpace(fila.Dui))
                        {
                            // 1. Limpiamos cualquier basura (espacios, guiones mal puestos)
                            string duiLimpio = fila.Dui.Replace("-", "").Trim();

                            // 2. Si tiene 9 dígitos exactos (formato sin guion), lo arreglamos.
                            //    Ejemplo: "000162975" -> se convierte en "00016297-5"
                            if (duiLimpio.Length == 9 && long.TryParse(duiLimpio, out _))
                            {
                                fila.Dui = duiLimpio.Insert(8, "-"); 
                            }
                            // Si ya traía guion o está incompleto, lo dejamos pasar para validarlo abajo.
                        }
                        // =======================================================

                        // A. VALIDACIÓN DE DUPLICADOS (Usando versión limpia para comparar)
                        string duiParaComparar = fila.Dui?.Replace("-", "").Trim() ?? "";

                        if (setDuisExistentes.Contains(duiParaComparar))
                        {
                            listaErrores.Add($"Fila {filasProcesadas}: El DUI {fila.Dui} ya existe en el sistema.");
                            continue;
                        }

                        if (setDuisEnEsteArchivo.Contains(duiParaComparar))
                        {
                            listaErrores.Add($"Fila {filasProcesadas}: El DUI {fila.Dui} está duplicado dentro del mismo archivo.");
                            continue;
                        }

                        // B. VALIDACIÓN DE MODELO
                        // Ahora que ya le pusimos el guion arriba (si le faltaba), esta validación pasará exitosamente.
                        var contextoValidacion = new ValidationContext(fila);
                        var resultadosValidacion = new List<ValidationResult>();
                        bool esValido = Validator.TryValidateObject(fila, contextoValidacion, resultadosValidacion, true);

                        if (!esValido)
                        {
                            var msjs = string.Join(", ", resultadosValidacion.Select(r => r.ErrorMessage));
                            listaErrores.Add($"Fila {filasProcesadas} ({fila.Dui}): {msjs}");
                            continue;
                        }

                        var nuevoBeneficiario = new Beneficiario
                        {
                            Nombre = fila.Nombre,
                            Dui = fila.Dui, // Aquí ya va formateado correctamente con guion
                            Direccion = fila.Direccion,
                            Telefono = fila.Telefono,
                            Genero = fila.Genero,
                            FechaNacimiento = fila.FechaNacimiento ?? DateTime.Parse("1900-01-01"), 
                            EstadoSubsidio = "pendiente",
                            UsuarioId = null
                        };

                        nuevosBeneficiarios.Add(nuevoBeneficiario);
                        setDuisEnEsteArchivo.Add(duiParaComparar);
                    }
                }

                if (nuevosBeneficiarios.Count > 0)
                {
                    await _context.Beneficiarios.AddRangeAsync(nuevosBeneficiarios);
                    await _context.SaveChangesAsync();
                }

                ViewBag.Exito = $"Proceso terminado. Se guardaron {nuevosBeneficiarios.Count} beneficiarios nuevos.";
                
                if (listaErrores.Count > 0)
                {
                    ViewBag.Advertencia = $"Se encontraron errores en {listaErrores.Count} registros.";
                    ViewBag.ListaErrores = listaErrores.Take(100).ToList(); 
                }
            }
            catch (CsvHelperException ex)
            {
                ViewBag.Error = $"Error de formato en el CSV (Fila {ex.Context.Parser.Row}): {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en carga masiva");
                ViewBag.Error = $"Ocurrió un error inesperado: {ex.Message}";
            }

            return View("CargaMasiva");
        }

        // ==========================================
        // 📤 MÓDULO DE EXPORTACIÓN (VERSIÓN BLINDADA PARA RED INESTABLE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportarListado()
        {
            try 
            {
                // 1. CONFIGURACIÓN CRÍTICA: Permitir IO Síncrono
                var syncIOFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
                if (syncIOFeature != null)
                {
                    syncIOFeature.AllowSynchronousIO = true;
                }

                var nombreArchivo = $"ListadoBeneficiarios_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                
                Response.ContentType = "text/csv; charset=utf-8";
                Response.Headers.Add("Content-Disposition", $"attachment; filename={nombreArchivo}");
                
                _context.Database.SetCommandTimeout(300); 

                using (var writer = new StreamWriter(Response.Body, new UTF8Encoding(true), bufferSize: 4096, leaveOpen: true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    var query = _context.Beneficiarios
                        .AsNoTracking()
                        .OrderBy(b => b.Nombre)
                        .Select(b => new BeneficiarioImportDto
                        {
                            Nombre = b.Nombre,
                            Dui = b.Dui,
                            Direccion = b.Direccion,
                            Telefono = b.Telefono,
                            Genero = b.Genero,
                            FechaNacimiento = b.FechaNacimiento
                        })
                        .AsAsyncEnumerable(); 

                    await csv.WriteRecordsAsync(query);
                    await writer.FlushAsync();
                }

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico exportando listado");
                
                if (!Response.HasStarted)
                {
                    return BadRequest("No se pudo iniciar la descarga. Revisa los logs del servidor.");
                }
                return new EmptyResult(); 
            }
        }
    }
}