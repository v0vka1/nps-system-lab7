using Nps.Domain.Enums;

namespace Nps.Services.DTOs;

/// <summary>Публічне представлення пристрою для відповідей REST API.</summary>
public sealed record DeviceDto(
    Guid Id,
    string Name,
    DeviceType Type,
    bool IsEnabled,
    decimal? Temperature,
    DateTime LastUpdated);