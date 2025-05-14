using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Computer_Club.Models;

namespace Computer_Club.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context  = context;
    }

    public IActionResult Index()
    {
        var computers = _context.Computers.ToList();
        var usersCount = _context.Users.Count(c => !c.IsAdmin);
        var freeComputersCount = _context.Computers.Count(c => !c.IsBooked);
        var productsCount = _context.Products.Count();

        var viewModel = new HomeCurrentCompUser()
        {
            Computers = computers,
            RegisteredUsersCount = usersCount,
            FreeComputersCount = freeComputersCount,
            ProductsCount  = productsCount
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    
}