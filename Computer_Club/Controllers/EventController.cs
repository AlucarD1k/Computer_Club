using Computer_Club.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Computer_Club.Controllers;

public class EventController : Controller
{
    private readonly ILogger<EventController> _logger;
    private readonly ApplicationDbContext _context;

    public EventController(ILogger<EventController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    
    public IActionResult Index()
    {
        var events = _context.Events.ToList();
        return View(events);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        var newEvent = new Event()
        {
            EventStartTime = DateTime.Now,
            EventEndTime = DateTime.Now.AddHours(1)
        };
        return View(newEvent);
    }
    
    [HttpPost]
    public IActionResult Create([Bind("EventName,EventDescription,EventStartTime,EventEndTime")] Event newEvent)
    {
        if (ModelState.IsValid)
        {
            if (newEvent.EventStartTime >= newEvent.EventEndTime)
            {
                // Добавляем ошибку без привязки к конкретному полю
                ModelState.AddModelError(string.Empty, "Дата начала не может быть позже даты окончания.");
                return View("Create",newEvent);
            }
            _context.Events.Add(newEvent);
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
        return View(newEvent);
    }
    
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var eventToEdit = _context.Events.FirstOrDefault(e => e.EventId == id);
        if (eventToEdit == null)
        {
            return NotFound();
        }
    
        return View(eventToEdit);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("EventId,EventName,EventDescription,EventStartTime,EventEndTime")] Event updatedEvent)
    {
        if (id != updatedEvent.EventId)
            return NotFound();
    
        if (ModelState.IsValid)
        {
            _context.Events.Update(updatedEvent);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    
        return View(updatedEvent);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, string _method)
    {
        if (_method == "DELETE")
        {
            var eventToDelete = _context.Events.Find(id);
            if (eventToDelete == null)
            {
                return NotFound();
            }
    
            _context.Events.Remove(eventToDelete);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    
        return BadRequest(); // если не DELETE
    }
    
    // GET: /Event/AddToEvent?eventId=...
    [HttpGet] 
    public IActionResult AddToEvent(int eventId) 
    {
        // Находим событие по идентификатору
        var ev = _context.Events.Find(eventId);
        if (ev == null)
        {
            return NotFound();
        }
            
        var allUsers = _context.Users.Where(user => user.IsAdmin == false).ToList();
        var registeredUsersIds = _context.EventUsers
            .Where(e => e.EventId == eventId)
            .Select(e => e.UserId)
            .ToList();
        // Создаём ViewModel и заполняем список всех пользователей (в реальном приложении здесь могут быть дополнительные фильтры)
        var model = new AddToEventViewModel
        {
            EventId = ev.EventId,
            EventName = ev.EventName,
            AllUsers = allUsers,
            SelectedUserIds = registeredUsersIds
        };
    
        return View(model);
    }
    
    // POST: /Event/AddToEvent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToEvent(AddToEventViewModel model)
    {
        // Если ни один чекбокс не выбран, берем пустой список вместо null
        var selectedUserIds = model.SelectedUserIds ?? new List<int>();
    
        if (ModelState.IsValid)
        {
            // Загружаем существующие записи для этого события
            var currentRegistrations = _context.EventUsers
                .Where(eu => eu.EventId == model.EventId)
                .ToList();
    
            var currentUserIds = currentRegistrations.Select(eu => eu.UserId).ToList();
    
            // Найдём пользователей, которых нужно добавить: те, что выбраны, но отсутствуют в базе регистрации
            var toAdd = selectedUserIds.Except(currentUserIds).ToList();
            // Найдём пользователей, которых нужно удалить: те, что есть в базе, но не выбраны в форме
            var toRemove = currentUserIds.Except(selectedUserIds).ToList();
    
            // Добавляем новые регистрации
            foreach (var userId in toAdd)
            {
                _context.EventUsers.Add(new EventUser
                {
                    EventId = model.EventId,
                    UserId = userId
                });
            }
    
            // Удаляем те регистрации, которые не выбраны
            var registrationsToRemove = currentRegistrations
                .Where(eu => toRemove.Contains(eu.UserId))
                .ToList();
            foreach (var reg in registrationsToRemove)
            {
                _context.EventUsers.Remove(reg);
            }
    
            _context.SaveChanges();
            TempData["SuccessMessage"] = "User registrations updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    
        // Если ModelState не валидна, восстановим список пользователей для повторного отображения
        model.AllUsers = _context.Users.Where(u => u.IsAdmin == false).ToList();
        return View(model);
    }
    

    // GET: /Event/Search
    [HttpGet]
    public IActionResult Search()
    {
        // Загружаем данные и сохраняем в сессию
        var list = _context.Events
            .Select(e => new
            {
                e.EventName,
                e.EventDescription,
                Start = e.EventStartTime,
                End = e.EventEndTime
            })
            .ToList();

        HttpContext.Session.SetString("Events", JsonSerializer.Serialize(list));

        return View();
    }

    // GET: /Event/SessionEventsJson
    [HttpGet]
    public IActionResult SessionEventsJson()
    {
        var json = HttpContext.Session.GetString("Events");
        if (string.IsNullOrEmpty(json))
            return Json(new List<object>());

        var events = JsonSerializer.Deserialize<List<object>>(json);
        return Json(events);
    }
}