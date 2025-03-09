using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // ������������ �������
        options.JsonSerializerOptions.WriteIndented = true; // ������������� JSON (�����������) ����� �� ���� ���� � �������
    });

var app = builder.Build();
app.MapControllers();

app.Run();