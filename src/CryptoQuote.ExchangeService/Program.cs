using CryptoQuote.ExchangeService.Endpoints;
using CryptoQuote.ExchangeService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console());

    builder.Services.AddHttpClient<IExchangeRateClient, ExchangeRateClient>(client =>
    {
        var baseUrl = builder.Configuration["ExchangeRates:BaseUrl"]
            ?? "http://api.exchangeratesapi.io/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
    });

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.MapExchangeEndpoints();

    Log.Information("ExchangeService starting");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ExchangeService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
