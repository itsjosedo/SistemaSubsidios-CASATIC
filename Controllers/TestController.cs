using Microsoft.AspNetCore.Mvc;
using SistemaSubsidios_CASATIC.Services;

public class TestController : Controller
{
    private readonly EmailService _email;

    public TestController(EmailService email)
    {
        _email = email;
    }

    public async Task<IActionResult> ProbarCorreo()
    {
        await _email.EnviarCorreo(
            "josedocallesgmz@gmail.com",
            "Prueba desde sistema",
            "<h2>Esto es una prueba de envío de correo.</h2>"
        );

        return Content("✔ Correo enviado (si no llega, revisa spam).");
    }
}