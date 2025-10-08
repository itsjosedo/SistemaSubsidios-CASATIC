using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BaseController : Controller
    {
        protected string GetRolUsuario()
        {
            return User.FindFirstValue("Rol") ?? User.FindFirstValue(ClaimTypes.Role) ?? "";
        }

        protected string GetNombreUsuario()
        {
            return User.FindFirstValue(ClaimTypes.Name) ?? "";
        }

        protected int? GetUserId()
        {
            var userId = User.FindFirstValue("UserId");
            return string.IsNullOrEmpty(userId) ? null : int.Parse(userId);
        }

        protected int? GetEntidadId()
        {
            var entidadId = User.FindFirstValue("EntidadId");
            return string.IsNullOrEmpty(entidadId) ? null : int.Parse(entidadId);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var rol = GetRolUsuario();
            var rolNormalizado = rol?.Trim().ToLower() ?? "";
            
            ViewBag.Layout = rolNormalizado switch
            {
                "admin" or "administrador" => "_LayoutAdmin",
                "entidad" or "operador" => "_LayoutEntidad", 
                "beneficiario" => "_LayoutBeneficiario",
                _ => "_Layout"
            };

            ViewBag.RolUsuario = rol;
            ViewBag.NombreUsuario = GetNombreUsuario();
            ViewBag.UserId = GetUserId();
            ViewBag.EntidadId = GetEntidadId();

            base.OnActionExecuting(context);
        }
    }
}