using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Nps.API.Health;
using Nps.API.HostedServices;
using Nps.API.Middleware;
using Nps.API.Validators;
using Nps.Domain.Interfaces;
using Nps.Infrastructure.Data;
using Nps.Infrastructure.Repositories;
using Nps.Services.Interfaces;
using Nps.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Кирилиця в JSON-відповідях залишається читабельною.
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ЛАБ7: CORS, щоб веб-клієнт (інший домен на Render) міг звертатися до API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ЛАБ7: рядок підключення до PostgreSQL береться ЛИШЕ із конфігурації середовища
// (user-secrets локально, змінна ConnectionStrings__NpsDatabase у хмарі). У коді його немає.
var connectionString = builder.Configuration.GetConnectionString("NpsDatabase")
    ?? throw new InvalidOperationException("Не налаштовано рядок підключення NpsDatabase.");

// ЛАБ7: провайдер PostgreSQL (Npgsql) замість SQLite; повтор запиту при короткочасних збоях мережі.
builder.Services.AddDbContext<NpsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

// DI: контролер -> IDeviceService -> IDeviceRepository -> NpsDbContext (PostgreSQL).
builder.Services.AddScoped<IDeviceRepository, EfDeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

// ЛАБ5: реєстрація всіх валідаторів із збірки Nps.API через DI.
builder.Services.AddValidatorsFromAssemblyContaining<CreateDeviceRequestValidator>();

// ЛАБ5: застосування міграцій і початкові дані під час запуску.
builder.Services.AddHostedService<DatabaseInitializer>();
// ЛАБ5: перевірки стану API та бази даних.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db" });

var app = builder.Build();

// ЛАБ5: централізована обробка помилок. Має стояти першою, щоб ловити помилки наступних компонентів.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ЛАБ7: CORS. Стоїть одразу після обробки помилок, до маршрутів.
app.UseCors("WebClient");

app.UseSwagger();
app.UseSwaggerUI();

// ЛАБ7: з кореня сайту переходимо на Swagger (сторінка застосунку за публічним посиланням).
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();
// ЛАБ5: /health показує, що API працює; /health/db додатково перевіряє базу даних.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

// ЛАБ5: тимчасовий маршрут для безпечної перевірки 500-помилки. Працює лише в Development.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/throw", () =>
    {
        throw new InvalidOperationException("Тестова помилка для перевірки middleware.");
    });
}

app.Run();