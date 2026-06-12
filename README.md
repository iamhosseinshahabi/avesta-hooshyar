# CryptoQuote

A **.NET 9 microservices solution** that accepts a cryptocurrency symbol (e.g. `BTC`) and returns its current price in **USD, EUR, BRL, GBP, and AUD**.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                    Client / Browser                 │
└──────────────────────────┬──────────────────────────┘
                           │ GET /api/cryptoquote/{code}
                           ▼
┌─────────────────────────────────────────────────────┐
│              CryptoQuote.API  (port 8080)           │
│   ASP.NET Core Web API · CQRS (MediatR) · EF Core  │
│   Swagger UI  ·  Global Exception Middleware        │
└────────────┬───────────────────────┬────────────────┘
             │                       │
    HTTP     │                       │  HTTP
             ▼                       ▼
┌──────────────────┐     ┌──────────────────────────┐
│  CoinService     │     │  ExchangeService          │
│  (port 5001)     │     │  (port 5002)              │
│  Wraps           │     │  Wraps                    │
│  CoinMarketCap   │     │  exchangeratesapi.io       │
└──────────────────┘     └──────────────────────────┘
             │
             ▼
┌──────────────────────────┐
│  SQL Server 2022         │
│  (CryptoQuoteDb)         │
│  Code-First / EnsureCreated│
└──────────────────────────┘
```

### Design patterns and principles

| Concern | Decision |
|---|---|
| Separation of concerns | Clean Architecture (Domain → Application → Infrastructure → API) |
| Request handling | CQRS with MediatR (`GetCryptoQuoteQuery`) |
| Cross-cutting | MediatR Pipeline Behaviours: `LoggingBehavior`, `ValidationBehavior` |
| Input validation | FluentValidation |
| Persistence | EF Core Code-First, SQL Server, Repository pattern |
| Service-to-service | Typed `HttpClient` wrappers |
| Error handling | Global `ExceptionHandlingMiddleware` |
| Logging | Serilog (structured, console sink) |
| Containerisation | Docker + Docker Compose |
| Testing | xUnit + Moq + FluentAssertions |

---

## Quick start — Docker Compose (one step)

```bash
# 1. Copy the environment file and fill in your API keys
cp .env.example .env

# 2. Edit .env — add your real keys:
#    COINMARKETCAP_API_KEY=<key from https://coinmarketcap.com/api>
#    EXCHANGERATES_ACCESS_KEY=<key from https://exchangeratesapi.io>

# 3. Build and run everything (SQL Server + two microservices + API)
docker-compose up --build
```

The API is then available at **http://localhost:8080**.  
Swagger UI: **http://localhost:8080/swagger**

> The database (`CryptoQuoteDb`) is created automatically on first run via `EnsureCreated()`.

---

## Quick start — local development

Prerequisites: .NET 9 SDK, a SQL Server instance.

```bash
# Terminal 1 – CoinService
cd src/CryptoQuote.CoinService
dotnet run   # listens on http://localhost:5001

# Terminal 2 – ExchangeService
cd src/CryptoQuote.ExchangeService
dotnet run   # listens on http://localhost:5002

# Terminal 3 – API Gateway
cd src/CryptoQuote.API
dotnet run   # listens on http://localhost:5000
```

Configure API keys in `appsettings.Development.json` (or via user secrets):

```json
// src/CryptoQuote.CoinService/appsettings.Development.json
{ "CoinMarketCap": { "ApiKey": "YOUR_KEY" } }

// src/CryptoQuote.ExchangeService/appsettings.Development.json
{ "ExchangeRates": { "AccessKey": "YOUR_KEY" } }
```

---

## Example request

```http
GET http://localhost:8080/api/cryptoquote/BTC
```

```json
{
  "cryptoCode": "BTC",
  "cryptoName": "Bitcoin",
  "quotes": {
    "USD": 68420.50,
    "EUR": 63142.30,
    "BRL": 349000.00,
    "GBP": 54120.80,
    "AUD": 104500.00
  },
  "retrievedAtUtc": "2024-06-11T09:15:00Z"
}
```

---

## Running the tests

```bash
dotnet test
```

Tests cover:

- `GetCryptoQuoteQueryHandler` — happy path, unknown symbol, null exchange response, persistence verification
- `GetCryptoQuoteQueryValidator` — valid/invalid codes
- `CoinMarketCapClient` — JSON deserialization, missing symbol, API error, missing config
- `ExchangeRateClient` — EUR-to-USD rebasing logic, API failure, USD always present

---

## API keys

| Service | Free tier | Header / parameter |
|---|---|---|
| CoinMarketCap | https://coinmarketcap.com/api | `X-CMC_PRO_API_KEY` header |
| ExchangeRates API | https://exchangeratesapi.io | `access_key` query parameter |

> **Note on ExchangeRates free tier:** the free plan uses EUR as the base currency and supports HTTP only (not HTTPS). The `ExchangeRateClient` rebases all rates to USD internally so the rest of the system always works in USD as the reference currency.

---

## Project structure

```
CryptoQuote/
├── src/
│   ├── CryptoQuote.CoinService/        # Microservice: CoinMarketCap wrapper
│   ├── CryptoQuote.ExchangeService/    # Microservice: ExchangeRates wrapper
│   └── CryptoQuote.API/                # API Gateway (CQRS + EF Core)
│       ├── Domain/                     # Entities, Interfaces, Exceptions
│       ├── Application/                # MediatR Query + Handler + Validators + Behaviors
│       ├── Infrastructure/             # DbContext, Repositories, HttpClients
│       ├── Controllers/
│       └── Middleware/
├── tests/
│   └── CryptoQuote.UnitTests/
│       ├── Application/
│       ├── CoinService/
│       └── ExchangeService/
├── docker-compose.yml
├── nuget.config
└── Answers to technical questions.md
```
