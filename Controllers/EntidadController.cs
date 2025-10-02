using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Models
{
    public class EntidadController : Controller
    {
        private readonly AppDbContext _db;

        public EntidadController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            var entidades = _db.Entidades.ToList();
            return View(entidades);
        }

        [HttpGet]
        public IActionResult CreateEntidad()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateEntidad(EntidadViewModel model)
        {
                if (ModelState.IsValid)
                {
                    var identidad = new Entidad
                    {
                        Nombre = model.Nombre,
                        Email = model.Email,
                        Direccion = model.Direccion
                    };
                    _db.Entidades.Add(identidad);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
            }
            return View("CreateEntidad", model);
        }
    }
}
