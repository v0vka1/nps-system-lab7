using Nps.Domain.Entities;

namespace Nps.Domain.Interfaces;

/// <summary>Контракт сховища пристроїв, незалежний від способу зберігання.</summary>
public interface IDeviceRepository
{
    Task<PlantDevice?> GetByIdAsync(Guid id);
    Task<IEnumerable<PlantDevice>> GetAllAsync();
    Task AddAsync(PlantDevice device);
    Task<bool> UpdateAsync(PlantDevice device);
    Task<bool> DeleteAsync(Guid id);
}
