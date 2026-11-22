using Nps.Services.DTOs;
using Nps.Services.Requests;

namespace Nps.Services.Interfaces;

/// <summary>Операції бізнес-рівня, які використовує REST-контролер.</summary>
public interface IDeviceService
{
    Task<IReadOnlyList<DeviceDto>> SearchDevicesAsync(string? name);
    Task<DeviceDto?> GetDeviceDetailsAsync(Guid id);
    Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request);
    Task<DeviceDto?> UpdateDeviceAsync(Guid id, UpdateDeviceRequest request);
    Task<bool> SetDeviceStateAsync(Guid id, bool isActive);
    Task<bool> DeleteDeviceAsync(Guid id);
}
