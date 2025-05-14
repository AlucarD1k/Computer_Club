namespace Computer_Club.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAdmin { get; set; }
    
    //Навигационные свойства 
    
    public ICollection<EventUser> EventUsers { get; set; } = new List<EventUser>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<UserOrder> UserOrders { get; set; } = new List<UserOrder>();
}