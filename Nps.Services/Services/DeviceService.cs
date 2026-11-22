using System.ComponentModel.DataAnnotations;
using Nps.Domain.Entities;
using Nps.Domain.Interfaces;
using Nps.Services.DTOs;
using Nps.Services.Interfaces;
using Nps.Services.Requests;

namespace Nps.Services.Services;

/// <summary>Бізнес-логіка роботи з пристроями; контролер не звертається до репозиторію напряму.</summary>
public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<IReadOnlyList<DeviceDto>> SearchDevicesAsync(string? name)
    {
        var devices = await _repository.GetAllAsync();
        var query = name?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            return devices.Select(MapToDto).ToList();
        }

        return devices
            .Where(device => device.Name.Contains(
                query, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto)
            .ToList();
    }

    public async Task<DeviceDto?> GetDeviceDetailsAsync(Guid id)
    {
        var device = await _repository.GetByIdAsync(id);
        return device is null ? null : MapToDto(device);
    }

    public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var device = new PlantDevice(request.Name, request.Type);
        device.RecordTemperature(request.Temperature);
        await _repository.AddAsync(device);
        return MapToDto(device);
    }

    public async Task<DeviceDto?> UpdateDeviceAsync(Guid id, UpdateDeviceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var device = await _repository.GetByIdAsync(id);
        if (device is null)
        {
            return null;
        }

        device.UpdateDetails(request.Name, request.Type);
        device.RecordTemperature(request.Temperature);
        var updated = await _repository.UpdateAsync(device);
        return updated ? MapToDto(device) : null;
    }

    public async Task<bool> SetDeviceStateAsync(Guid id, bool isActive)
    {
        var device = await _repository.GetByIdAsync(id);
        if (device is null)
        {
            return false;
        }

        // Передаємо бажаний стан, а не перемикаємо його: повторний PATCH безпечний.
        if (isActive)
        {
            device.Activate();
        }
        else
        {
            device.Deactivate();
        }

        return await _repository.UpdateAsync(device);
    }

    public Task<bool> DeleteDeviceAsync(Guid id) => _repository.DeleteAsync(id);

    private static void Validate(object request)
    {
        Validator.ValidateObject(
            request,
            new ValidationContext(request),
            validateAllProperties: true);
    }

    private static DeviceDto MapToDto(PlantDevice device) => new(
        device.Id,
        device.Name,
        device.Type,
        device.IsEnabled,
        device.Temperature,
        device.LastUpdated);
}