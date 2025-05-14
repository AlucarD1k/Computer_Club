namespace Computer_Club.Models;

public class Event
{
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string EventDescription { get; set; }
    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    
    //Навигационные поля
    public ICollection<EventUser> EventUsers { get; set; } = new List<EventUser>();
}