using Nps.Domain.Enums;

namespace Nps.Services.Requests;

/// <summary>Повний набір редагованих характеристик пристрою для PUT. Правила перевірки: у валідаторі API.</summary>
public sealed class UpdateDeviceRequest
{
    public string Name { get; init; } = string.Empty;

    public DeviceType Type { get; init; }

    /// <summary>Температура, °C. Відсутнє значення скидає її в null (PUT замінює весь набір).</summary>
    public decimal? Temperature { get; init; }
}