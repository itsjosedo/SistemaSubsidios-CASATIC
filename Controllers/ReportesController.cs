using Microsoft.AspNetCore.Mvc;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class ReportesController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
