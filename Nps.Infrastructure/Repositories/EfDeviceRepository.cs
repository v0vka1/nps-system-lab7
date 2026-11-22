using Microsoft.EntityFrameworkCore;
using Nps.Domain.Entities;
using Nps.Domain.Interfaces;
using Nps.Infrastructure.Data;

namespace Nps.Infrastructure.Repositories;

/// <summary>Постійне сховище пристроїв на SQLite та Entity Framework Core.</summary>
public class EfDeviceRepository : IDeviceRepository
{
    private readonly NpsDbContext _context;

    public EfDeviceRepository(NpsDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<PlantDevice?> GetByIdAsync(Guid id)
    {
        return await _context.Devices.FirstOrDefaultAsync(device => device.Id == id);
    }

    public async Task<IEnumerable<PlantDevice>> GetAllAsync()
    {
        IEnumerable<PlantDevice> devices = await _context.Devices
            .AsNoTracking()
            .OrderBy(device => device.Name)
            .ToListAsync();
        return devices;
    }

    public async Task AddAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(PlantDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var exists = await _context.Devices.AnyAsync(item => item.Id == device.Id);
        if (!exists)
        {
            return false;
        }

        if (_context.Entry(device).State == EntityState.Detached)
        {
            _context.Devices.Update(device);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(item => item.Id == id);
        if (device is null)
        {
            return false;
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }
}