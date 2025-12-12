using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class LogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public LogService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task Registrar(int? usuarioId, string accion, string? datos = null)
    {
        var ip = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        var log = new Log
        {
            UsuarioId = usuarioId,
            Accion = accion,
            Fecha = DateTime.Now,
            Ip = ip,
            Datos = datos
        };

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();
    }
}
