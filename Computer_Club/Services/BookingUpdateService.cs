using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Computer_Club.Models;

public class BookingUpdateService : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private Timer _timer;

    public BookingUpdateService(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.Now;

        // Удаляем завершившиеся брони
        var endedBookings = db.Bookings
            .Where(b => b.EndTime < now)
            .ToList();

        db.Bookings.RemoveRange(endedBookings);

        // Обновляем статус компьютеров
        var busyComputerIds = db.Bookings
            .Where(b => b.StartTime <= now && b.EndTime > now)
            .Select(b => b.ComputerId)
            .Distinct()
            .ToList();

        var allComputers = db.Computers.ToList();
        foreach (var comp in allComputers)
        {
            comp.IsBooked = busyComputerIds.Contains(comp.Id);
        }

        db.SaveChanges();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}