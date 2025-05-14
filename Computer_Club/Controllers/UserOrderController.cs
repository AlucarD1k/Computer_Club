using Computer_Club.Models;
using Computer_Club.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Computer_Club.Controllers;

public class UserOrderController : Controller
{
    private readonly ILogger<UserOrderController> _logger;
    private readonly ApplicationDbContext _context;
    
    public UserOrderController(ILogger<UserOrderController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var orders = _context.UsersOrders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
        
        return View(orders);
    }

    public IActionResult Details(int id)
    {
        var  order = _context.UsersOrders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id);
        
        if  (order == null) return NotFound();
        return View(order);
    }

    
    public IActionResult Delete(int id, string _method)
    {
        if (_method != "DELETE") return BadRequest();
        
        var order = _context.UsersOrders
            .Include(o => o.OrderItems)
            .FirstOrDefault(o => o.Id == id);
        if (order == null) return NotFound();
        
        // Сначала удаляем позиции, затем сам заказ
        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.UsersOrders.Remove(order);
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Order #{id} has been removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Create()
    {
        
        var vm = new OrderCreateViewModel
        {
            Products = _context.Products
                .Select(p => new ProductOrderViewModel {
                    ProductId = p.Id,
                    Name      = p.Name,
                    Price     = p.Price,
                    Quantity  = 0
                })
                .ToList()
        };
            
        ViewBag.Users = _context.Users
            .Where(u => !u.IsAdmin)
            .OrderBy(u => u.UserName)
            .ToList();
        ViewBag.UserList = new SelectList(
            _context.Users.Where(u => !u.IsAdmin),
            "UserId", "UserName"
        );

        return View(vm);
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(OrderCreateViewModel vm)
    {
        // Валидация: обязательно выбрать пользователя
        if (!ModelState.IsValid)
        {
            var users = _context.Users.Where(u => !u.IsAdmin).OrderBy(u => u.UserName).ToList();
            ViewBag.Users = users;
            ViewBag.UserList = new SelectList(users, "UserId", "UserName");
            return View(vm);

        }

        // Сформируем список позиций с qty>0
        var items = vm.Products
            .Where(x => x.Quantity > 0)
            .ToList();

        if (!items.Any())
        {
            ModelState.AddModelError(string.Empty, "Please specify quantity for at least one product.");
            var users = _context.Users.Where(u => !u.IsAdmin).OrderBy(u => u.UserName).ToList();
            ViewBag.Users = users;
            ViewBag.UserList = new SelectList(users, "UserId", "UserName");
            return View(vm);

        }

        // Общая сумма
        decimal total = items.Sum(x => x.Price * x.Quantity);

        // Создаём заказ
        var order = new UserOrder
        {
            UserId    = vm.UserId,
            OrderDate = DateTime.Now,
            Total     = total
        };
        _context.UsersOrders.Add(order);
        _context.SaveChanges(); // чтобы получить order.Id

        // Добавляем позиции
        foreach (var it in items)
        {
            _context.OrderItems.Add(new OrderItem {
                UserOrderId = order.Id,
                ProductId   = it.ProductId,
                Quantity    = it.Quantity
            });
        }
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Order #{order.Id} created, total {total:C}.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var order = _context.UsersOrders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id);

        if (order == null) return NotFound();

        // Получаем список всех продуктов заранее
        var products = _context.Products.ToList();

        var vm = new OrderEditViewModel
        {
            OrderId = order.Id,
            Products = products
                .Select(p => new ProductOrderViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = order.OrderItems
                        .FirstOrDefault(oi => oi.ProductId == p.Id)?.Quantity ?? 0
                })
                .ToList()
        };

        return View(vm);

    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(OrderEditViewModel vm)
    {
        var order = _context.UsersOrders
            .Include(o => o.OrderItems)
            .FirstOrDefault(o => o.Id == vm.OrderId);

        if (order == null) return NotFound();

        // Удаляем старые позиции
        _context.OrderItems.RemoveRange(order.OrderItems);

        var newItems = vm.Products
            .Where(p => p.Quantity > 0)
            .ToList();

        foreach (var item in newItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                UserOrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        // Пересчитываем сумму
        order.Total = newItems.Sum(p => p.Quantity * p.Price);
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Order #{order.Id} updated.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }


}