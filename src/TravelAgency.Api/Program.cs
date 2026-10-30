using Asp.Versioning;
using Scalar.AspNetCore;
using TravelAgency.Api.Filters;
using TravelAgency.Application;
using TravelAgency.Infrastructure;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using TravelAgency.Application.Common.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем Контроллеры + наш глобальный ActionFilter и настройки JSON
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ActionExecutionTimingFilter>();
});

// 2. Настраиваем Enterprise-версионирование API (например: /api/v1/diagnostics/status)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// 3. Настраиваем глобальный стандарт ошибок RFC 9457 (ProblemDetails)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddOpenApi();

// Регистрируем слои
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// === НАСТРОЙКА СТРУКТУРИРОВАННЫХ ЛОГОВ (SERILOG -> CONSOLE + GRAFANA LOKI) ===
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TravelAgency.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <TraceId:{TraceId}>{NewLine}{Exception}")
        .WriteTo.GrafanaLoki(
            context.Configuration["Observability:LokiUrl"] ?? "http://localhost:3100",
            labels:
            [
                new LokiLabel { Key = "app", Value = "travelagency-api" },
                new LokiLabel { Key = "env", Value = context.HostingEnvironment.EnvironmentName }
            ]);
});

// === НАСТРОЙКА МЕТРИК И ТРЕЙСОВ (OPENTELEMETRY -> PROMETHEUS) ===
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("TravelAgency.Api"))
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(TravelAgencyMetrics.MeterName) // Наши бизнес-метрики туров и выручки!
            .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

var app = builder.Build();

// === КОНВЕЙЕР MIDDLEWARE ===
app.UseExceptionHandler();

app.MapPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TravelAgency Enterprise API");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

// Открывает эндпоинт /metrics, откуда Prometheus каждые 5 секунд забирает показатели

app.Run();