using SistemaSubsidios_CASATIC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BeneficiariosController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BeneficiariosController> _logger;

        public BeneficiariosController(AppDbContext context, ILogger<BeneficiariosController> logger)
        {
            _context = context;
            _logger = logger;
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

            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // ==========================================
        // 📥 MÓDULO DE IMPORTACIÓN CSV (LO QUE FALTABA)
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

            int guardados = 0;
            int errores = 0;
            List<string> listaErrores = new List<string>();

            try
            {
                using (var stream = new StreamReader(archivoCSV.OpenReadStream()))
                using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                {
                    var registros = csv.GetRecords<BeneficiarioImportDto>();

                    foreach (var fila in registros)
                    {
                        bool existe = await _context.Beneficiarios.AnyAsync(b => b.Dui == fila.Dui);
                        if (existe)
                        {
                            errores++;
                            listaErrores.Add($"DUI duplicado: {fila.Dui} ({fila.Nombre})");
                            continue;
                        }

                        var nuevoBeneficiario = new Beneficiario
                        {
                            Nombre = fila.Nombre,
                            Dui = fila.Dui,
                            Direccion = fila.Direccion,
                            Telefono = fila.Telefono,
                            Genero = fila.Genero,
                            FechaNacimiento = fila.FechaNacimiento ?? DateTime.Now,
                            EstadoSubsidio = "pendiente",
                            UsuarioId = null
                        };

                        _context.Beneficiarios.Add(nuevoBeneficiario);
                        guardados++;
                    }
                    await _context.SaveChangesAsync();
                }

                ViewBag.Exito = $"Proceso terminado. Se guardaron {guardados} beneficiarios nuevos.";
                if (errores > 0)
                {
                    ViewBag.Advertencia = $"Hubo {errores} registros no guardados.";
                    ViewBag.ListaErrores = listaErrores;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ocurrió un error: {ex.Message}";
            }

            return View("CargaMasiva");
        }

        // ==========================================
        // 📤 MÓDULO DE EXPORTACIÓN (LO QUE FALTABA)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportarListado()
        {
            var beneficiarios = await _context.Beneficiarios
                .Select(b => new
                {
                    b.Nombre,
                    b.Dui,
                    b.Telefono,
                    b.Direccion,
                    b.Genero,
                    FechaNacimiento = b.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Estado = b.EstadoSubsidio
                })
                .ToListAsync();

            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(beneficiarios);
                writer.Flush();
                return File(memory.ToArray(), "text/csv", $"ListadoBeneficiarios_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            }
        }
    }
}