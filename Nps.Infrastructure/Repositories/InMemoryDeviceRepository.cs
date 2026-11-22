using Nps.Domain.Entities;
using Nps.Domain.Interfaces;

namespace Nps.Infrastructure.Repositories;

/// <summary>Сховище пристроїв в оперативній пам'яті на основі List.</summary>
public class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly List<PlantDevice> _devices = new();
    private readonly object _sync = new();

    public Task<PlantDevice?> GetByIdAsync(Guid id)
    {
        lock (_sync)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(device);
        }
    }

    public Task<IEnumerable<PlantDevice>> GetAllAsync()
    {
        lock (_sync)
        {
            IEnumerable<PlantDevice> devices = _devices.ToList();
            return Task.FromResult(devices);
        }
    }

    public Task AddAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        lock (_sync)
        {
            if (_devices.Any(d => d.Id == device.Id))
            {
                throw new InvalidOperationException(
                    $"Пристрій із Id '{device.Id}' вже існує.");
            }

            _devices.Add(device);
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        lock (_sync)
        {
            // PlantDevice зберігається за посиланням; метод перевіряє наявність запису.
            return Task.FromResult(_devices.Any(d => d.Id == device.Id));
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_sync)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
            {
                return Task.FromResult(false);
            }

            _devices.Remove(device);
            return Task.FromResult(true);
        }
    }
}
