using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Computer_Club.Models;

namespace Computer_Club.Controllers;

public class RegistrationController  : Controller
{
    private readonly ApplicationDbContext _context;

    public RegistrationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(string userName, string email, string password)
    {
        if (ModelState.IsValid)
        {
            
        }
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Please fill in all the fields");
            return View();
        }
        
        var existingUser = _context.Users.FirstOrDefault(x => x.UserName == userName && x.Email == email);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "User already exists");
            return View();
        }
        
        var passwordHash = ComputeSha256Hash(password);

        var newUser = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.Now,
            IsAdmin = false
        };
        
        _context.Users.Add(newUser);
        _context.SaveChanges();
        
        return RedirectToAction("Index", "Home");
    }
    
    // Простой метод для вычисления SHA256-хеша
    private string ComputeSha256Hash(string rawData)
    {
        using (var sha256Hash = SHA256.Create())
        {
            // Вычисляем хеш в виде байтов
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                
            // Преобразуем байты в строку в шестнадцатеричном виде
            var builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}