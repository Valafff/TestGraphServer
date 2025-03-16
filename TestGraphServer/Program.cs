var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // ������������ null-��������
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; // ������������� JSON (�����������)
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy() // ������������ camelCase
        };
    });

var app = builder.Build();

app.MapControllers();
app.Run();