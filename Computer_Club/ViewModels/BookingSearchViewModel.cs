using Computer_Club.Models;

namespace Computer_Club.ViewModels;

public class BookingSearchViewModel
{
    public List<Booking> Bookings { get; set; } = new();
    public int? SelectedComputerId { get; set; }
    public DateTime? SelectedDate { get; set; }

    public List<Computer> Computers { get; set; } = new();
}