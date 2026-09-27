using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TechStore.Api.Middleware;
using TechStore.Api.Modules.Catalog;
using TechStore.Api.Modules.Categories;

var builder = WebApplication.CreateBuilder(args);

// Key Vault — deve ser o primeiro provider para que Serilog e módulos já leiam os segredos
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    var aiConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(aiConnectionString) && !aiConnectionString.StartsWith('<'))
    {
        config.WriteTo.ApplicationInsights(aiConnectionString, TelemetryConverter.Traces);
    }
});

// Módulos
builder.Services.AddCategoriesModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TechStore API",
        Version = "v1",
        Description = "API do sistema TechStore Cloud — Cadastro de Produtos"
    });
});

// CORS
var allowedOrigins = builder.Configuration["AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["*"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Contains("*"))
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CategoriesDbContext>("categories-db")
    .AddDbContextCheck<CatalogDbContext>("catalog-db");

// Application Insights
var aiConnStr = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(aiConnStr) && !aiConnStr.StartsWith('<'))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = aiConnStr;
    });
}

var app = builder.Build();

// Aplicar migrations automaticamente
// Em produção, o correto seria executar migrations em um passo separado de CI/CD,
// nunca no startup da aplicação, para evitar problemas com múltiplas instâncias
// e permitir rollback controlado. Para este MVP, a praticidade justifica.
using (var scope = app.Services.CreateScope())
{
    var categoriesDb = scope.ServiceProvider.GetRequiredService<CategoriesDbContext>();
    categoriesDb.Database.Migrate();

    var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    catalogDb.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors();

// Em produção real, o Swagger ficaria desabilitado ou protegido por autenticação.
// Como este é um MVP acadêmico, mantemos habilitado para facilitar testes e capturas de tela.
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

app.MapCategoriesEndpoints();
app.MapCatalogEndpoints();

app.Run();
