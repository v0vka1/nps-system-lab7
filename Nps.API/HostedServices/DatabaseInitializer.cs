using Microsoft.EntityFrameworkCore;
using Nps.Domain.Entities;
using Nps.Domain.Enums;
using Nps.Infrastructure.Data;

namespace Nps.API.HostedServices;

/// <summary>Під час запуску API застосовує міграції та за потреби додає початкові пристрої.</summary>
public sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceProvider services,
        ILogger<DatabaseInitializer> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NpsDbContext>();

            await context.Database.MigrateAsync(cancellationToken);

            if (!await context.Devices.AnyAsync(cancellationToken))
            {
                context.Devices.AddRange(
                    new PlantDevice("Насос аварійного охолодження", DeviceType.Actuator),
                    new PlantDevice("Датчик температури активної зони", DeviceType.Sensor));
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("База даних порожня: додано два початкові пристрої.");
            }

            _logger.LogInformation("База даних готова до роботи.");
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Не вдалося підготувати базу даних під час запуску API.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}