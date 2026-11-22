using Nps.Domain.Enums;

namespace Nps.Domain.Entities;

/// <summary>Пристрій у системі моніторингу енергоблоків АЕС.</summary>
public class PlantDevice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public DeviceType Type { get; private set; }
    public bool IsEnabled { get; private set; }

    /// <summary>Остання відома температура, °C. Null, якщо пристрій її не вимірює.</summary>
    public decimal? Temperature { get; private set; }

    /// <summary>Час останньої зміни запису в UTC.</summary>
    public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;

    public PlantDevice(string name, DeviceType type)
    {
        ValidateName(name);
        Name = name.Trim();
        Type = type;
        IsEnabled = false;
        Touch();
    }

    /// <summary>Активує пристрій.</summary>
    public void Activate()
    {
        IsEnabled = true;
        Touch();
    }

    /// <summary>Деактивує пристрій.</summary>
    public void Deactivate()
    {
        IsEnabled = false;
        Touch();
    }

    /// <summary>Оновлює редаговані характеристики пристрою.</summary>
    public void UpdateDetails(string name, DeviceType type)
    {
        ValidateName(name);
        Name = name.Trim();
        Type = type;
        Touch();
    }

    /// <summary>Записує останнє значення температури (або null).</summary>
    public void RecordTemperature(decimal? temperature)
    {
        Temperature = temperature;
        Touch();
    }

    private void Touch() => LastUpdated = DateTime.UtcNow;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Назва пристрою не може бути порожньою.",
                nameof(name));
        }
    }
}