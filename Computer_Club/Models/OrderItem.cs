namespace Computer_Club.Models;

public class OrderItem
{
    public int UserOrderId { get; set; }
    public UserOrder UserOrder { get; set; }
    
    public int ProductId { get; set; }
    public Product Product { get; set; }
    
    public int Quantity { get; set; }
}