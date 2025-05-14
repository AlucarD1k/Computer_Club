namespace Computer_Club.Models;

public class AddToEventViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
        
    // Список всех пользователей для показа в форме
    public List<User> AllUsers { get; set; } = new List<User>();

    // Список выбранных пользователями ID, которые пользователь оставил отмеченными
    public List<int> SelectedUserIds { get; set; } = new List<int>();
}