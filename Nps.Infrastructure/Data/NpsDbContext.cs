using Microsoft.EntityFrameworkCore;
using Nps.Domain.Entities;

namespace Nps.Infrastructure.Data;

/// <summary>Контекст бази даних SQLite для пристроїв системи моніторингу енергоблоків.</summary>
public sealed class NpsDbContext : DbContext
{
    public NpsDbContext(DbContextOptions<NpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlantDevice> Devices => Set<PlantDevice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlantDevice>(entity =>
        {
            // Обмеження цілісності на рівні сховища (діють незалежно від валідації в API).
            entity.ToTable("Devices", table =>
            {
                table.HasCheckConstraint(
                    "CK_Devices_Name_Length",
                    "length(\"Name\") BETWEEN 2 AND 100");
                table.HasCheckConstraint(
                    "CK_Devices_Type",
                    "\"Type\" IN ('Sensor', 'Actuator')");
            });

            entity.HasKey(device => device.Id);

            entity.Property(device => device.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Тип пристрою зберігається текстом (Sensor / Actuator).
            entity.Property(device => device.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Новий пристрій за замовчуванням вимкнений.
            entity.Property(device => device.IsEnabled)
                .HasDefaultValue(false);

            // Необов'язкове поле: до 4 цифр до коми та 2 після.
            entity.Property(device => device.Temperature)
                .HasPrecision(6, 2);

            // Час зберігається в UTC; після читання з бази позначається як UTC.
            entity.Property(device => device.LastUpdated)
                .IsRequired()
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

            // Індекс для пошуку за назвою.
            entity.HasIndex(device => device.Name)
                .HasDatabaseName("IX_Devices_Name");
        });
    }
}