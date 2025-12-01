using SistemaSubsidios_CASATIC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using CsvHelper;
using CsvHelper.Configuration; // <-- Necesaria para configurar la lectura
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
                .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios del beneficiario
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

            // ✅ CAMBIADO: Obtener subsidios desde la relación muchos-a-muchos
            var subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>();

            // ✅ 2️⃣ Enviar los subsidios mediante ViewBag
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
                // ✅ AGREGAR: Información de subsidios
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
                .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                beneficiario = new Beneficiario
                {
                    UsuarioId = userId.Value
                };
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
                // ✅ AGREGAR: Información de subsidios
                Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CompletarPerfil(BeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                _logger.LogWarning("Errores de validación: {Errores}", string.Join(", ", errores));
                ViewBag.Errores = errores;
                return View(model);
            }

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                // ✅ Buscamos el beneficiario del usuario
                var beneficiario = await _context.Beneficiarios
                    .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios
                    .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

                // ✅ Si no existe, lo creamos
                if (beneficiario == null)
                {
                    beneficiario = new Beneficiario
                    {
                        UsuarioId = userId.Value,
                    };
                    _context.Beneficiarios.Add(beneficiario);
                }

                var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id_Usuario == userId.Value);

                if (usuario != null)
                {
                    beneficiario.Nombre = usuario.Nombre;
                }

                // ✅ Aquí sí asignamos los datos del formulario
                beneficiario.Dui = model.Dui?.Trim();
                beneficiario.Telefono = model.Telefono?.Trim();
                beneficiario.Direccion = model.Direccion?.Trim();
                beneficiario.Genero = model.Genero?.Trim();
                beneficiario.FechaNacimiento = model.FechaNacimiento;

                // ✅ Guardamos los cambios
                await _context.SaveChangesAsync();

                Console.WriteLine("Guardado exitoso, redirigiendo...");
                Console.WriteLine($"Dui: {beneficiario.Dui}, Teléfono: {beneficiario.Telefono}, Dirección: {beneficiario.Direccion}");
                TempData["MensajeExito"] = "Tu perfil se ha completado correctamente.";
                return RedirectToAction("Index", "Beneficiarios");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar el perfil del beneficiario");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos.");
                return View(model);
            }
        }

        // GET: Beneficiarios/Lista (Vista pública para el dashboard)
        public async Task<IActionResult> Lista()
        {
            var beneficiarios = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios
                .Where(b => !string.IsNullOrEmpty(b.Dui) && !string.IsNullOrEmpty(b.Nombre))
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewData["Title"] = "Lista de Beneficiarios";
            return View(beneficiarios);
        }

        //GET: Editar Perfil beneficiario
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
                return RedirectToAction("CompletarPerfil");

            var model = new BeneficiarioViewModel
            {
                Telefono = beneficiario.Telefono,
                Direccion = beneficiario.Direccion,
                // ✅ AGREGAR: Información de subsidios
                Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>()
            };

            return View(model);
        }

        // POST: Beneficiarios/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(BeneficiarioViewModel model)
        {
            Console.WriteLine("Método POST EditarPerfil ejecutado");

            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscamos al beneficiario ANTES de validar
            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)  // ← CAMBIADO: Incluir subsidios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                return RedirectToAction("CompletarPerfil");
            }

            // ✅ Validamos solo lo que realmente se está editando
            ModelState.Remove("Nombre");
            ModelState.Remove("Dui");
            ModelState.Remove("Correo");
            ModelState.Remove("Genero");
            ModelState.Remove("FechaNacimiento");

            if (!ModelState.IsValid)
            {
                // ✅ Reasignamos datos actuales para que no se pierdan en la vista
                model.Telefono = beneficiario.Telefono;
                model.Direccion = beneficiario.Direccion;
                model.Subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>();
                return View(model);
            }

            // ✅ Guardamos los cambios permitidos
            beneficiario.Telefono = model.Telefono?.Trim();
            beneficiario.Direccion = model.Direccion?.Trim();

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // ==============================================
        // 🆕 MÉTODOS NUEVOS PARA GESTIÓN DE SUBSIDIOS
        // ==============================================

        // GET: Beneficiarios/MisSubsidios
        public async Task<IActionResult> MisSubsidios()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)  // Obtener todos los subsidios del beneficiario
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                TempData["WarningMessage"] = "Primero debe completar su perfil de beneficiario";
                return RedirectToAction("CompletarPerfil");
            }

            return View(beneficiario.Subsidios?.ToList() ?? new List<Subsidio>());
        }

        // GET: Beneficiarios/DetallesSubsidio/5
        public async Task<IActionResult> DetallesSubsidio(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                TempData["WarningMessage"] = "Beneficiario no encontrado";
                return RedirectToAction("Index");
            }

            var subsidio = beneficiario.Subsidios?.FirstOrDefault(s => s.Id == id);
            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado o no pertenece a este beneficiario";
                return RedirectToAction("MisSubsidios");
            }

            return View(subsidio);
        }

        // GET: Beneficiarios/Estadisticas
        public async Task<IActionResult> Estadisticas()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Subsidios)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                return RedirectToAction("CompletarPerfil");
            }

            var subsidios = beneficiario.Subsidios?.ToList() ?? new List<Subsidio>();

            var estadisticas = new
            {
                TotalSubsidios = subsidios.Count,
                SubsidiosActivos = subsidios.Count(s => s.Estado == "Activo"),
                SubsidiosPendientes = subsidios.Count(s => s.Estado == "Pendiente"),
                MontoTotal = subsidios.Sum(s => s.Monto),
                UltimoSubsidio = subsidios.OrderByDescending(s => s.FechaAsignacion).FirstOrDefault()
            };

            ViewBag.Estadisticas = estadisticas;
            return View();
        }

        // ==========================================
        // 📥 MÓDULO DE IMPORTACIÓN CSV (NUEVO)
        // ==========================================

        // 1. GET: Mostrar la pantalla para subir el archivo
        [HttpGet]
        public IActionResult CargaMasiva()
        {
            return View();
        }

        // 2. POST: Recibir y procesar el archivo
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
                // Leemos el archivo usando un flujo de memoria (Stream)
                using (var stream = new StreamReader(archivoCSV.OpenReadStream()))
                using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                {
                    // Leemos los registros usando nuestro molde (DTO)
                    var registros = csv.GetRecords<BeneficiarioImportDto>();

                    foreach (var fila in registros)
                    {
                        // VERIFICACIÓN 1: ¿Ya existe este DUI en la base de datos?
                        bool existe = await _context.Beneficiarios.AnyAsync(b => b.Dui == fila.Dui);
                        
                        if (existe)
                        {
                            errores++;
                            listaErrores.Add($"DUI duplicado: {fila.Dui} ({fila.Nombre})");
                            continue; // Saltamos al siguiente registro
                        }

                        // Si no existe, creamos la entidad real
                        var nuevoBeneficiario = new Beneficiario
                        {
                            Nombre = fila.Nombre,
                            Dui = fila.Dui,
                            Direccion = fila.Direccion,
                            Telefono = fila.Telefono,
                            Genero = fila.Genero,
                            FechaNacimiento = fila.FechaNacimiento ?? DateTime.Now,
                            EstadoSubsidio = "pendiente",
                            UsuarioId = null // Importante: Queda null porque viene de Excel
                        };

                        _context.Beneficiarios.Add(nuevoBeneficiario);
                        guardados++;
                    }
                    
                    // Guardamos todos los cambios en la base de datos de una sola vez
                    await _context.SaveChangesAsync();
                }

                ViewBag.Exito = $"Proceso terminado. Se guardaron {guardados} beneficiarios nuevos.";
                if (errores > 0)
                {
                    ViewBag.Advertencia = $"Hubo {errores} registros que no se guardaron (probablemente DUIs repetidos).";
                    ViewBag.ListaErrores = listaErrores;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ocurrió un error al leer el archivo: {ex.Message}";
            }

            return View("CargaMasiva");
        }

        // ==========================================
        // 📤 MÓDULO DE EXPORTACIÓN (NUEVO)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportarListado()
        {
            // 1. Obtenemos los datos de la BD
            var beneficiarios = await _context.Beneficiarios
                .Select(b => new
                {
                    b.Nombre,
                    b.Dui,
                    b.Telefono,
                    b.Direccion,
                    b.Genero,
                    // Damos formato a la fecha para que se lea bien en Excel
                    FechaNacimiento = b.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Estado = b.EstadoSubsidio
                })
                .ToListAsync();

            // 2. Generamos el archivo en memoria
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(beneficiarios);
                writer.Flush();
                
                // 3. Devolvemos el archivo al usuario
                return File(memory.ToArray(), "text/csv", $"ListadoBeneficiarios_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            }
        }
    }
}