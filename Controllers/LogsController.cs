using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class LogsController : Controller
{
    private readonly AppDbContext _db;

    public LogsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _db.Logs
            .OrderByDescending(l => l.Fecha)
            .ToListAsync();

        return View(logs);
    }
}
