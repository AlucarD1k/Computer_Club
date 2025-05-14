using Computer_Club.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Computer_Club.Controllers;

public class MenuController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public MenuController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var products = _context.Products.ToList();
        return View(products);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    public IActionResult Create([Bind("Name,Price")] Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return View(product);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Name,Price")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == product.Id))
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
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, string _method)
    {
        if (_method == "DELETE")
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError(e, "Ошибка при удалении позиции"); // Логирование ошибки
                return StatusCode(500, "Ошибка при удалении позиции"); // Возвращаем ошибку
            }
        }
        return BadRequest();
    }
}