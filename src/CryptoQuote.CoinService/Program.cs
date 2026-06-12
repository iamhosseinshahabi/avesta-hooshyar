using CryptoQuote.CoinService.Endpoints;
using CryptoQuote.CoinService.Services;
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

    builder.Services.AddHttpClient<ICoinMarketCapClient, CoinMarketCapClient>(client =>
    {
        var baseUrl = builder.Configuration["CoinMarketCap:BaseUrl"]
            ?? "https://pro-api.coinmarketcap.com/";
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
    });

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.MapCoinEndpoints();

    Log.Information("CoinService starting on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CoinService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
