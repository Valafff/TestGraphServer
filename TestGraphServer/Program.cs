using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Игнорировать регистр
        options.JsonSerializerOptions.WriteIndented = true; // Форматировать JSON (опционально) чтобы не было каши в консоли
    });

var app = builder.Build();
app.MapControllers();

app.Run();