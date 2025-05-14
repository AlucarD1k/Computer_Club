namespace Computer_Club.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    
    public int ComputerId { get; set; }
    public Computer  Computer { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}