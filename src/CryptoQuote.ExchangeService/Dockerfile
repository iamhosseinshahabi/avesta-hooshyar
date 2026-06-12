FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/CryptoQuote.ExchangeService/CryptoQuote.ExchangeService.csproj", "src/CryptoQuote.ExchangeService/"]
RUN dotnet restore "src/CryptoQuote.ExchangeService/CryptoQuote.ExchangeService.csproj"
COPY . .
WORKDIR "/src/src/CryptoQuote.ExchangeService"
RUN dotnet build "CryptoQuote.ExchangeService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoQuote.ExchangeService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoQuote.ExchangeService.dll"]
