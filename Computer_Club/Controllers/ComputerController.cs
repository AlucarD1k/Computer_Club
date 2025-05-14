using Computer_Club.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Computer_Club.Controllers;

public class ComputerController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public ComputerController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var computers = _context.Computers.ToList();
        return View(computers);
    }
    
    [HttpGet]
    public IActionResult Create() 
    {
        return View(new Computer());
    }
        
    [HttpPost]
    public IActionResult Create([Bind("Name,Characteristics,IsBooked")] Computer computer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(computer);
            _context.SaveChanges();
            Console.WriteLine("Computer created successfully!");
            return RedirectToAction(nameof(Index));
        }
        else
        {
            // Вывод ошибок в консоль для диагностики
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return View(computer);
    }

    

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        
        var computer = _context.Computers.Find(id);
        if (computer == null)
        {
            return NotFound();
        }
        
        return View(computer);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public  IActionResult Edit(int id, [Bind("Id,Name,Characteristics,IsBooked")] Computer computer)
    {
        if (id != computer.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(computer);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Computers.Any(e => e.Id == computer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(computer);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public  IActionResult Delete(int id, string _method)
    {
        // Проверяем, что пришел метод удаления
        if (_method == "DELETE")
        {
            var computer =  _context.Computers.Find(id);

            if (computer == null)
            {
                return NotFound(); // Если объект не найден, возвращаем 404
            }

            try
            {
                _context.Computers.Remove(computer);
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