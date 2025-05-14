using System.ComponentModel.DataAnnotations;
namespace Computer_Club.ViewModels;

public class BookingCreateViewModel
{
    [Required(ErrorMessage = "Please select a user.")]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Please select a computer.")]
    [Display(Name = "Computer")]
    public int ComputerId { get; set; }

    [Required(ErrorMessage = "Please enter a start time.")]
    [Display(Name = "Start At")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Please enter an end time.")]
    [Display(Name = "End At")]
    public DateTime EndTime { get; set; }
}