using Computer_Club.Models;
namespace Computer_Club.ViewModels;

public class EventSearchViewModel
{
    // фильтры
    public string NameFilter      { get; set; }
    public DateTime? FromDate     { get; set; }
    public DateTime? ToDate       { get; set; }

    // список результатов
    public List<Event> Results    { get; set; } = new();

    // (вдруг пригодятся для других списков)
    public List<Event> AllEvents  { get; set; } = new();
}