using Nps.Domain.Enums;

namespace Nps.Services.Requests;

/// <summary>Дані, необхідні для створення пристрою. Правила перевірки: у валідаторі API.</summary>
public sealed class CreateDeviceRequest
{
    public string Name { get; init; } = string.Empty;

    public DeviceType Type { get; init; }

    /// <summary>Необов'язкова початкова температура, °C.</summary>
    public decimal? Temperature { get; init; }
}