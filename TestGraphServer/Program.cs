var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // Игнорировать null-значения
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; // Форматировать JSON (опционально)
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy() // Использовать camelCase
        };
    });

var app = builder.Build();

app.MapControllers();
app.Run();