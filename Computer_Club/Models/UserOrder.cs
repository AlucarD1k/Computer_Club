namespace Computer_Club.Models;

public class UserOrder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; }
}