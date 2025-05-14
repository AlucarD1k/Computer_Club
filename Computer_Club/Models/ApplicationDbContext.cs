using Microsoft.EntityFrameworkCore;

namespace Computer_Club.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Computer>  Computers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Event>  Events { get; set; }
    public DbSet<Product>  Products { get; set; }
    public DbSet<UserOrder>  UsersOrders { get; set; }
    public DbSet<OrderItem>  OrderItems { get; set; }
    public DbSet<EventUser>   EventUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настраиваем составной ключ для EventUser (связь многие ко многим между Events и Users)
        modelBuilder.Entity<EventUser>()
            .HasKey(eu => new { eu.EventId, eu.UserId });
        
        //Составной ключь для OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.UserOrderId, oi.ProductId });
        
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.UserOrder)
            .WithMany(uo => uo.OrderItems)
            .HasForeignKey(oi => oi.UserOrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

    }
}