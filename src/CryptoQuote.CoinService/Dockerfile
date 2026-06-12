FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/CryptoQuote.CoinService/CryptoQuote.CoinService.csproj", "src/CryptoQuote.CoinService/"]
RUN dotnet restore "src/CryptoQuote.CoinService/CryptoQuote.CoinService.csproj"
COPY . .
WORKDIR "/src/src/CryptoQuote.CoinService"
RUN dotnet build "CryptoQuote.CoinService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoQuote.CoinService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoQuote.CoinService.dll"]
