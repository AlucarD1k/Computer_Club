using Computer_Club.Models;
using Microsoft.AspNetCore.Mvc;

namespace Computer_Club.Controllers;

public class UserOrderController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public UserOrderController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
}