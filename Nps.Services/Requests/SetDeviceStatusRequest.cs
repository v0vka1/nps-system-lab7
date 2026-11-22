namespace Nps.Services.Requests;

/// <summary>Бажаний стан пристрою для часткового оновлення через PATCH.</summary>
public sealed class SetDeviceStatusRequest
{
    /// <summary>true — активувати, false — деактивувати. Поле обов'язкове.</summary>
    public bool? IsActive { get; init; }
}