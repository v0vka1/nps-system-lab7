using FluentValidation;
using Nps.Services.Requests;

namespace Nps.API.Validators;

/// <summary>Правила перевірки запиту на створення пристрою.</summary>
public sealed class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва пристрою є обов'язковою.")
            .Length(2, 100).WithMessage("Назва пристрою має містити від 2 до 100 символів.");

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage("Тип пристрою повинен бути Sensor або Actuator.");

        RuleFor(request => request.Temperature)
            .Must(value => !value.HasValue || (value.Value >= -50m && value.Value <= 400m))
            .WithMessage("Температура має бути в діапазоні від -50 до 400 °C.");
    }
}

/// <summary>Правила перевірки запиту на повне оновлення пристрою.</summary>
public sealed class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва пристрою є обов'язковою.")
            .Length(2, 100).WithMessage("Назва пристрою має містити від 2 до 100 символів.");

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage("Тип пристрою повинен бути Sensor або Actuator.");

        RuleFor(request => request.Temperature)
            .Must(value => !value.HasValue || (value.Value >= -50m && value.Value <= 400m))
            .WithMessage("Температура має бути в діапазоні від -50 до 400 °C.");
    }
}

/// <summary>Правила перевірки запиту на зміну стану пристрою.</summary>
public sealed class SetDeviceStatusRequestValidator : AbstractValidator<SetDeviceStatusRequest>
{
    public SetDeviceStatusRequestValidator()
    {
        RuleFor(request => request.IsActive)
            .NotNull().WithMessage("Поле isActive є обов'язковим: true або false.");
    }
}