using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nps.Infrastructure.Data;

namespace Nps.API.Health;

/// <summary>Перевіряє, чи API може підключитися до бази даних SQLite.</summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly NpsDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(NpsDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("База даних доступна.")
                : HealthCheckResult.Unhealthy("База даних недоступна.");
        }
        catch (Exception exception)
        {
            // Деталі йдуть лише в журнал, клієнт їх не бачить.
            _logger.LogError(exception, "Перевірка стану бази даних завершилась помилкою.");
            return HealthCheckResult.Unhealthy("База даних недоступна.");
        }
    }
}