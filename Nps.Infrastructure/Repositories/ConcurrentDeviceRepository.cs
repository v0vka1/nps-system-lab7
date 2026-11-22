using System.Collections.Concurrent;
using Nps.Domain.Entities;
using Nps.Domain.Interfaces;

namespace Nps.Infrastructure.Repositories;

/// <summary>Альтернативне потокобезпечніше in-memory сховище на ConcurrentDictionary.</summary>
public class ConcurrentDeviceRepository : IDeviceRepository
{
    private readonly ConcurrentDictionary<Guid, PlantDevice> _devices = new();

    public Task<PlantDevice?> GetByIdAsync(Guid id)
    {
        _devices.TryGetValue(id, out var device);
        return Task.FromResult(device);
    }

    public Task<IEnumerable<PlantDevice>> GetAllAsync()
    {
        IEnumerable<PlantDevice> devices = _devices.Values.ToList();
        return Task.FromResult(devices);
    }

    public Task AddAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (!_devices.TryAdd(device.Id, device))
        {
            throw new InvalidOperationException(
                $"Пристрій із Id '{device.Id}' вже існує.");
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return Task.FromResult(_devices.ContainsKey(device.Id));
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_devices.TryRemove(id, out _));
    }
}
