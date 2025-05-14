using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Computer_Club.Models; // Для SelectList или IEnumerable<Computer>

namespace Computer_Club.ViewModels;

public class BookingEditViewModel
{
    // ID бронирования (не редактируется, но нужен для отправки формы)
    public int Id { get; set; }

    // ID пользователя (не редактируется)
    public int UserId { get; set; }

    // Имя пользователя (только для отображения)
    [Display(Name = "Пользователь")]
    public string UserName { get; set; } = ""; // Инициализируем пустой строкой

    // Время начала (только для отображения)
    [Display(Name = "Начало брони")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime StartTime { get; set; }

    // --- Редактируемые поля ---

    [Required(ErrorMessage = "Пожалуйста, выберите компьютер.")]
    [Display(Name = "Компьютер")]
    public int ComputerId { get; set; }

    [Required(ErrorMessage = "Пожалуйста, укажите время окончания.")]
    [Display(Name = "Окончание брони")]
    public DateTime EndTime { get; set; }

    // --- Данные для формы ---

    // Список доступных компьютеров для выбора (можно передавать через ViewBag, как раньше)
    // Или можно раскомментировать и заполнять в контроллере:
    // public IEnumerable<Computer>? AvailableComputers { get; set; }
}