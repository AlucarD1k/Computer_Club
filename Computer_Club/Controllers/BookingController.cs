using Computer_Club.Models;
using Computer_Club.ViewModels; // Add this using statement
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Computer_Club.Controllers;

public class BookingController : Controller
{
    private readonly ILogger<BookingController> _logger;
    private readonly ApplicationDbContext _context;

    public BookingController(ILogger<BookingController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var bookings = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Computer)
            .Where(b => b.EndTime >= DateTime.Now)
            .OrderBy(b => b.EndTime)
            .ToList();

        return View(bookings);
    }

    // GET: /Booking/Create
    [HttpGet]
    public IActionResult Create()
    {
        _logger.LogInformation("GET: /Booking/Create called");

        // Create the ViewModel
        var viewModel = new BookingCreateViewModel
        {
            // Set default times
            StartTime = DateTime.Now.AddMinutes(15),
            EndTime = DateTime.Now.AddHours(1).AddMinutes(15)
        };

        // Load data for dropdowns into ViewBag (or ideally into the ViewModel itself)
        ViewBag.Users = _context.Users
            .Where(u => !u.IsAdmin)
            .OrderBy(u => u.UserName) // Good practice to order dropdowns
            .ToList();
        ViewBag.Computers = _context.Computers
            .OrderBy(c => c.Name)   // Good practice to order dropdowns
            .ToList();

        return View(viewModel); // Pass the ViewModel to the view
    }

    // POST: /Booking/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Accept the ViewModel instead of the Booking model
    public IActionResult Create(BookingCreateViewModel viewModel)
    {
        _logger.LogInformation("POST: /Booking/Create called. Attempting to book ComputerId {ComputerId} for UserId {UserId} from {StartTime} to {EndTime}",
            viewModel.ComputerId, viewModel.UserId, viewModel.StartTime, viewModel.EndTime);

        // --- Perform custom validation checks on the ViewModel ---
        // Note: Basic required field checks are handled by attributes on the ViewModel

        if (viewModel.StartTime < DateTime.Now.AddMinutes(-1))
        {
            ModelState.AddModelError(nameof(viewModel.StartTime), "Start time cannot be in the past.");
            _logger.LogWarning("Validation Failed: Start time in the past ({StartTime})", viewModel.StartTime);
        }

        if (viewModel.StartTime >= viewModel.EndTime)
        {
            ModelState.AddModelError(nameof(viewModel.EndTime), "End time must be after start time.");
             _logger.LogWarning("Validation Failed: End time ({EndTime}) not after start time ({StartTime})", viewModel.EndTime, viewModel.StartTime);
        }

        // Check for overlapping bookings
        bool isBusy = _context.Bookings.Any(b =>
            b.ComputerId == viewModel.ComputerId &&
            // No need to check b.Id != viewModel.Id since we are creating a new one
            b.EndTime > viewModel.StartTime &&
            b.StartTime < viewModel.EndTime
        );

        if (isBusy)
        {
            // Add error related to the specific fields or general model error
            ModelState.AddModelError(string.Empty, "This computer is already booked during the selected time period. Please choose a different time or computer.");
            _logger.LogWarning("Validation Failed: ComputerId {ComputerId} is busy during the requested time.", viewModel.ComputerId);
        }
        // --- End Validation Checks ---


        // Check ModelState AFTER custom checks
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Returning to Create view.");
            foreach (var modelStateKey in ModelState.Keys) // Log errors
            {
                var value = ModelState[modelStateKey];
                foreach (var error in value.Errors)
                {
                    _logger.LogWarning("ModelState Error: Key={Key}, Error={ErrorMessage}", modelStateKey, error.ErrorMessage);
                }
            }

            // Reload ViewBag data before returning the view
            ViewBag.Users = _context.Users.Where(u => !u.IsAdmin).OrderBy(u => u.UserName).ToList();
            ViewBag.Computers = _context.Computers.Where(c => !c.IsBooked).OrderBy(c => c.Name).ToList();
            return View(viewModel); // Return the view with the ViewModel
        }

        // --- Map ViewModel to Entity and Save ---
        // Create the actual Booking entity only if validation passes
        var booking = new Booking
        {
            UserId = viewModel.UserId,
            ComputerId = viewModel.ComputerId,
            StartTime = viewModel.StartTime,
            EndTime = viewModel.EndTime
            // The User and Computer navigation properties will be null here,
            // and that's okay because EF Core uses the foreign keys (UserId, ComputerId)
            // to establish the relationship when saving.
        };

        try
        {
            _logger.LogInformation("ModelState is valid. Attempting to save booking.");
            _context.Bookings.Add(booking);
            _context.SaveChanges();
            _logger.LogInformation("Booking saved successfully. Redirecting to Index.");

            TempData["SuccessMessage"] = $"Booking for computer {booking.ComputerId} from {booking.StartTime} to {booking.EndTime} created successfully!";
            return RedirectToAction("Index", "Booking");
        }
        catch (DbUpdateException ex)
        {
             _logger.LogError(ex, "Error saving booking to the database.");
             ModelState.AddModelError(string.Empty, "An error occurred while saving the booking. Please try again.");
             // Reload ViewBag data before returning view
             ViewBag.Users = _context.Users.Where(u => !u.IsAdmin).OrderBy(u => u.UserName).ToList();
             ViewBag.Computers = _context.Computers.Where(c => !c.IsBooked).OrderBy(c => c.Name).ToList();
             return View(viewModel); // Return view with ViewModel
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "An unexpected error occurred during booking creation.");
             ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
             // Reload ViewBag data before returning view
             ViewBag.Users = _context.Users.Where(u => !u.IsAdmin).OrderBy(u => u.UserName).ToList();
             ViewBag.Computers = _context.Computers.Where(c => !c.IsBooked).OrderBy(c => c.Name).ToList();
             return View(viewModel); // Return view with ViewModel
        }
    }
    // --- МЕТОДЫ РЕДАКТИРОВАНИЯ (EDIT) ---

    // GET: Booking/Edit/5
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        _logger.LogInformation("GET: Booking/Edit/{Id} called", id);
        if (id == null)
        {
            _logger.LogWarning("Edit GET called with null id");
            return NotFound();
        }

        // Находим бронь, включая пользователя и компьютер для отображения
        var booking = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Computer)
            .FirstOrDefault(b => b.Id == id);

        if (booking == null)
        {
             _logger.LogWarning("Booking with Id {Id} not found for Edit GET", id);
            return NotFound();
        }

        // Создаем ViewModel для редактирования
        var viewModel = new BookingEditViewModel
        {
            Id = booking.Id,
            UserId = booking.UserId,
            UserName = booking.User?.UserName ?? "N/A", // Отображаем имя пользователя
            ComputerId = booking.ComputerId,
            StartTime = booking.StartTime, // Время начала не редактируется
            EndTime = booking.EndTime      // Время окончания редактируется
        };

        // Загружаем список компьютеров для выбора
        ViewBag.Computers = _context.Computers
                                      .OrderBy(c => c.Name)
                                      .ToList(); // Загружаем все компьютеры

        return View(viewModel);
    }

    // POST: Booking/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, BookingEditViewModel viewModel)
    {
         _logger.LogInformation("POST: Booking/Edit/{Id} called", id);

        // Проверяем совпадение ID из маршрута и ViewModel
        if (id != viewModel.Id)
        {
             _logger.LogWarning("Mismatched Id in Edit POST. Route Id: {RouteId}, ViewModel Id: {ViewModelId}", id, viewModel.Id);
            return NotFound(); // Или BadRequest()
        }

        // Находим ОРИГИНАЛЬНУЮ бронь в базе данных
        var bookingToUpdate = _context.Bookings.Find(id);
        if (bookingToUpdate == null)
        {
             _logger.LogWarning("Booking with Id {Id} not found for Edit POST", id);
            return NotFound();
        }

        // --- Выполняем валидацию ---
        // 1. Проверка времени окончания относительно НЕИЗМЕНЕННОГО времени начала
        if (bookingToUpdate.StartTime >= viewModel.EndTime)
        {
            ModelState.AddModelError(nameof(viewModel.EndTime), "Время окончания должно быть после времени начала.");
            _logger.LogWarning("Validation Failed (Edit): End time ({EndTime}) not after start time ({StartTime})", viewModel.EndTime, bookingToUpdate.StartTime);
        }

        // 2. Проверка пересечений для НОВОГО компьютера и времени, ИСКЛЮЧАЯ ТЕКУЩУЮ бронь
        bool isBusy = _context.Bookings.Any(b =>
            b.Id != viewModel.Id && // Исключаем текущую бронь!
            b.ComputerId == viewModel.ComputerId && // Проверяем для выбранного компьютера
            b.EndTime > bookingToUpdate.StartTime && // Конец существующей > Начала редактируемой (неизменно)
            b.StartTime < viewModel.EndTime        // Начало существующей < Конца редактируемой (новое)
        );

        if (isBusy)
        {
            ModelState.AddModelError(string.Empty, "Выбранный компьютер уже занят на это время. Пожалуйста, выберите другое время или компьютер.");
            _logger.LogWarning("Validation Failed (Edit): ComputerId {ComputerId} is busy during the requested time.", viewModel.ComputerId);
        }
        // --- Конец валидации ---

        // Проверяем ModelState после своих проверок
        if (ModelState.IsValid)
        {
            // Обновляем ТОЛЬКО разрешенные поля
            bookingToUpdate.ComputerId = viewModel.ComputerId;
            bookingToUpdate.EndTime = viewModel.EndTime;

            try
            {
                _context.Update(bookingToUpdate); // Помечаем сущность как измененную
                _context.SaveChanges(); // Сохраняем изменения
                _logger.LogInformation("Booking {Id} updated successfully.", id);
                TempData["SuccessMessage"] = $"Бронь ID {id} успешно обновлена.";
                return RedirectToAction("Index", "Booking");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Обработка ошибок конкуренции (если кто-то изменил запись одновременно)
                 _logger.LogError(ex, "Concurrency error updating booking {Id}", id);
                 ModelState.AddModelError(string.Empty, "Не удалось сохранить изменения. Бронь была изменена другим пользователем. Пожалуйста, обновите страницу и попробуйте снова.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating booking {Id}", id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении бронирования в базе данных.");
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating booking {Id}", id);
                ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка при обновлении.");
            }
        }

        // Если ModelState не валиден или произошла ошибка сохранения:
        _logger.LogWarning("ModelState invalid or update failed for booking {Id}. Returning to Edit view.", id);
        // Перезагружаем необходимые данные для View
        ViewBag.Computers = _context.Computers.OrderBy(c => c.Name).ToList();
        // Восстанавливаем нередактируемые поля в ViewModel для корректного отображения
        viewModel.UserName = (_context.Users.Find(bookingToUpdate.UserId))?.UserName ?? "N/A";
        viewModel.StartTime = bookingToUpdate.StartTime;

        return View(viewModel); // Возвращаем пользователя на форму редактирования с ошибками
    }


    // --- МЕТОД УДАЛЕНИЯ (DELETE) ---
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id, string _method)
    {
        _logger.LogInformation("POST: Booking/Delete/{Id} called with _method: {Method}", id, _method);

        // Проверяем, что это действительно запрос на удаление
        if (_method?.ToUpper() != "DELETE") // Сделаем проверку нечувствительной к регистру
        {
             _logger.LogWarning("Delete POST called for Id {Id} but _method was not 'DELETE' ('{Method}')", id, _method);
            return BadRequest("Некорректный метод запроса для удаления.");
        }

        // Находим бронь для удаления
        var bookingToDelete = _context.Bookings.Find(id);

        if (bookingToDelete == null)
        {
             _logger.LogWarning("Booking with Id {Id} not found for Delete POST", id);
            // Можно вернуть NotFound, или Redirect с сообщением об ошибке
            TempData["ErrorMessage"] = $"Бронь с ID {id} не найдена.";
            return RedirectToAction("Index", "Booking");
        }

        try
        {
            _context.Bookings.Remove(bookingToDelete); // Помечаем на удаление
            _context.SaveChanges(); // Применяем удаление в базе
            _logger.LogInformation("Booking {Id} deleted successfully.", id);
            TempData["SuccessMessage"] = $"Бронь ID {id} успешно удалена.";
            return RedirectToAction("Index", "Booking");
        }
        catch (DbUpdateException ex) // Обработка ошибок БД (например, связанных с внешними ключами, если они есть и настроены на запрет удаления)
        {
            _logger.LogError(ex, "Database error deleting booking {Id}", id);
            // Сообщаем пользователю об ошибке
            TempData["ErrorMessage"] = $"Ошибка при удалении брони ID {id}. Возможно, есть связанные данные.";
            // Можно добавить более специфичную обработку ошибок внешних ключей, если нужно
            return RedirectToAction("Index", "Booking");
        }
        catch (Exception ex) // Обработка других непредвиденных ошибок
        {
             _logger.LogError(ex, "Unexpected error deleting booking {Id}", id);
             TempData["ErrorMessage"] = $"Непредвиденная ошибка при удалении брони ID {id}.";
             return RedirectToAction("Index", "Booking");
        }
    }
}

    
    /*// GET: /Booking/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users
                .Where(u => !u.IsAdmin)
                .ToList();

            ViewBag.Computers = _context.Computers
                .Where(c => !c.IsBooked)      // можно сразу показать только свободные ПК
                .ToList();

            var booking = new Booking
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            return View(booking);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("UserId,ComputerId,StartTime,EndTime")] Booking booking)
        {
            // Общие проверки
            if (booking.StartTime < DateTime.Now)
                ModelState.AddModelError(string.Empty, "Start time must be in the future.");

            if (booking.StartTime >= booking.EndTime)
                ModelState.AddModelError(string.Empty, "End time must be after start time.");

            // Проверка пересечений
            bool isBusy = _context.Bookings.Any(b =>
                b.ComputerId == booking.ComputerId &&
                b.EndTime > booking.StartTime &&
                b.StartTime < booking.EndTime
            );
            if (isBusy)
                ModelState.AddModelError(string.Empty, "This computer is already booked for the selected period.");

            if (!ModelState.IsValid)
            {
                // Если что‑то не так, шагаем обратно в форму, подгружая списки
                ViewBag.Users = _context.Users
                    .Where(u => !u.IsAdmin)
                    .ToList();
                ViewBag.Computers = _context.Computers
                    .Where(c => !c.IsBooked)
                    .ToList();
                return View(booking);
            }

            // Всё ок — сохраняем бронь и помечаем компьютер как занятый
            _context.Bookings.Add(booking);

            /*var comp = _context.Computers.Find(booking.ComputerId);
            if (comp != null) comp.IsBooked = true;#1#

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }*/

    /*[HttpGet]
    public IActionResult Create()
    {
        ViewBag.Computers = _context.Computers.ToList();
        
        ViewBag.Users = _context.Users
            .Where(u=>!u.IsAdmin)
            .ToList();
        
        var newBooking = new Booking()
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
        };
        return View(newBooking);
    }

    [HttpPost]
    public IActionResult Create(Booking booking)
    {
        if (booking.StartTime < DateTime.Now)
        {
            ModelState.AddModelError("", "Start time must be after current time.");
        }

        if (booking.StartTime >= booking.EndTime)
        {
            ModelState.AddModelError("", "End time must be after start time.");
        }

        bool isBusy = _context.Bookings.Any(b =>
            b.ComputerId == booking.ComputerId &&
            b.EndTime > booking.StartTime &&
            b.StartTime < booking.EndTime
        );

        if (isBusy)
        {
            ModelState.AddModelError("", "This computer is already booked at the selected time.");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.Users = _context.Users.Where(u => !u.IsAdmin).ToList();
            ViewBag.Computers = _context.Computers.ToList();
            return View(booking);
        }
        _context.Bookings.Add(booking);
        
        var computer = _context.Computers.FirstOrDefault(c => c.Id == booking.ComputerId);
        if (computer != null)
        {
            computer.IsBooked = true;
            _context.Computers.Update(computer);
        }
        
        _context.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }*/
