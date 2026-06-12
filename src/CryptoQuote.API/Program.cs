using System.Reflection;
using CryptoQuote.API.Application.Common.Behaviors;
using CryptoQuote.API.Application.Validators;
using CryptoQuote.API.Domain.Interfaces;
using CryptoQuote.API.Infrastructure.Data;
using CryptoQuote.API.Infrastructure.Data.Repositories;
using CryptoQuote.API.Infrastructure.HttpClients;
using CryptoQuote.API.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console());

    // MediatR + Pipeline Behaviors
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<Program>();
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<GetCryptoQuoteQueryValidator>();

    // EF Core – SQL Server (Code First, auto-create)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.EnableRetryOnFailure(5)));

    // Repositories
    builder.Services.AddScoped<IQuoteHistoryRepository, QuoteHistoryRepository>();

    // HttpClients for downstream microservices
    builder.Services.AddHttpClient<ICoinServiceClient, CoinServiceClient>(client =>
    {
        var url = builder.Configuration["Services:CoinServiceUrl"]
            ?? throw new InvalidOperationException("Services:CoinServiceUrl is not configured.");
        client.BaseAddress = new Uri(url.TrimEnd('/') + '/');
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    builder.Services.AddHttpClient<IExchangeServiceClient, ExchangeServiceClient>(client =>
    {
        var url = builder.Configuration["Services:ExchangeServiceUrl"]
            ?? throw new InvalidOperationException("Services:ExchangeServiceUrl is not configured.");
        client.BaseAddress = new Uri(url.TrimEnd('/') + '/');
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    // Controllers + Swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CryptoQuote API",
            Version = "v1",
            Description = "Real-time cryptocurrency quotes in USD, EUR, BRL, GBP, and AUD. " +
                          "Prices are sourced from CoinMarketCap; currency conversion is provided by ExchangeRates API.",
            Contact = new OpenApiContact
            {
                Name = "Hossein Shahabinejad",
                Email = "shahabinejad.m@gmail.com"
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

    var app = builder.Build();

    // Auto-create database on first run
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        Log.Information("Database ensured: {DbName}", db.Database.GetDbConnection().Database);
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoQuote API v1"));
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("CryptoQuote API starting");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CryptoQuote API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
