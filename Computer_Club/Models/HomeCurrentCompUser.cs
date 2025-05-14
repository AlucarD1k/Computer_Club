namespace Computer_Club.Models;

public class HomeCurrentCompUser 
{
    public IEnumerable<Computer> Computers { get; set; }
    public int RegisteredUsersCount { get; set; }
    public int FreeComputersCount { get; set; }
    
    public int ProductsCount { get; set; }
}