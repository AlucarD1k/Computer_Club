using Computer_Club.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Computer_Club.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public UserController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        if (await TryUpdateModelAsync(user, "",
                u => u.UserName, u => u.Email, u => u.IsAdmin))
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public  IActionResult Delete(int id, string _method)
    {
        // Проверяем, что пришел метод удаления
        if (_method == "DELETE")
        {
            var user =  _context.Users.Find(id);

            if (user == null)
            {
                return NotFound(); // Если объект не найден, возвращаем 404
            }

            try
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index)); // Перенаправляем обратно на страницу с компьютерами
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError(e, "Ошибка при удалении компьютера"); // Логирование ошибки
                return StatusCode(500, "Ошибка при удалении компьютера"); // Возвращаем ошибку
            }
        }

        return BadRequest(); // Если метод не DELETE, возвращаем ошибку
    }
}