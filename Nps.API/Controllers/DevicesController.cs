using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nps.Services.DTOs;
using Nps.Services.Interfaces;
using Nps.Services.Requests;

namespace Nps.API.Controllers;

/// <summary>REST-контролер пристроїв системи моніторингу енергоблоків.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IValidator<CreateDeviceRequest> _createValidator;
    private readonly IValidator<UpdateDeviceRequest> _updateValidator;
    private readonly IValidator<SetDeviceStatusRequest> _statusValidator;

    public DevicesController(
        IDeviceService deviceService,
        IValidator<CreateDeviceRequest> createValidator,
        IValidator<UpdateDeviceRequest> updateValidator,
        IValidator<SetDeviceStatusRequest> statusValidator)
    {
        _deviceService = deviceService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
    }

    /// <summary>Повертає всі пристрої або фільтрує їх за назвою.</summary>
    /// <param name="name">Необов'язкова частина назви пристрою.</param>
    /// <returns>Список пристроїв зі статусом 200 OK.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeviceDto>>> GetAll(
        [FromQuery] string? name)
    {
        var devices = await _deviceService.SearchDevicesAsync(name ?? string.Empty);
        return Ok(devices);
    }

    /// <summary>Повертає пристрій за його GUID.</summary>
    /// <param name="id">Унікальний ідентифікатор пристрою.</param>
    /// <returns>200 OK із пристроєм або 404 Not Found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDto>> GetById(Guid id)
    {
        var device = await _deviceService.GetDeviceDetailsAsync(id);
        return device is null ? NotFound() : Ok(device);
    }

    /// <summary>Створює новий пристрій.</summary>
    /// <param name="request">Назва та тип нового пристрою.</param>
    /// <returns>201 Created із посиланням на створений ресурс або 400 Bad Request.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeviceDto>> Create(
        [FromBody] CreateDeviceRequest request)
    {
        var invalid = await ValidateAsync(_createValidator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var created = await _deviceService.CreateDeviceAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Повністю оновлює назву та тип пристрою.</summary>
    /// <param name="id">Унікальний ідентифікатор пристрою.</param>
    /// <param name="request">Повний набір редагованих характеристик.</param>
    /// <returns>200 OK з оновленими даними, 400 Bad Request або 404 Not Found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDto>> Update(
        Guid id,
        [FromBody] UpdateDeviceRequest request)
    {
        var invalid = await ValidateAsync(_updateValidator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var updated = await _deviceService.UpdateDeviceAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Частково змінює стан пристрою, встановлюючи бажане значення.</summary>
    /// <param name="id">Унікальний ідентифікатор пристрою.</param>
    /// <param name="request">Бажаний стан: true — активувати, false — деактивувати.</param>
    /// <returns>204 No Content, 400 Bad Request або 404 Not Found.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        Guid id,
        [FromBody] SetDeviceStatusRequest request)
    {
        var invalid = await ValidateAsync(_statusValidator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var updated = await _deviceService.SetDeviceStateAsync(id, request.IsActive!.Value);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Видаляє пристрій зі сховища.</summary>
    /// <param name="id">Унікальний ідентифікатор пристрою.</param>
    /// <returns>204 No Content або 404 Not Found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deviceService.DeleteDeviceAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Запускає валідатор; якщо є помилки, повертає 400 зі списком помилок за полями.</summary>
    private async Task<ActionResult?> ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        if (result.IsValid)
        {
            return null;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return ValidationProblem(new ValidationProblemDetails(errors));
    }
}