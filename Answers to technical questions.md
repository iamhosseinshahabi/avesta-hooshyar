# Answers to Technical Questions

---

## 1. How long did you spend on the coding assignment? What would you add with more time?

I spent approximately **4–5 hours** on this assignment.

If I had more time I would add:

- **API Key rotation & secret management** via Azure Key Vault or HashiCorp Vault instead of environment variables.
- **Distributed caching** (Redis) to avoid hitting rate-limited external APIs on every request — cryptocurrency prices change infrequently enough that a 60-second TTL cache would improve both performance and cost.
- **Polly resilience policies** (retry with exponential back-off, circuit breaker, timeout) on all outbound HTTP calls to the two external APIs.
- **Health-check endpoints** (`/healthz/live` and `/healthz/ready`) for each microservice, wired into the Docker Compose `healthcheck` and Kubernetes liveness probes.
- **EF Core migrations** (instead of `EnsureCreated`) so schema changes can be applied safely in CI/CD pipelines.
- **Integration tests** with a real SQL Server instance (via `Testcontainers`) and stubbed external HTTP services (via WireMock.Net).
- **Rate-limiting middleware** on the API gateway to protect it from abuse.
- **OpenTelemetry** tracing across service calls so the full request waterfall is visible in tools like Jaeger or Azure Application Insights.
- **Authentication/Authorization** (e.g. JWT bearer) on the public endpoint.

---

## 2. Most useful feature added to .NET / C# recently

**C# 12 Primary Constructors** (shipped with .NET 8) are the feature I use most. They eliminate the repetitive boilerplate of declaring a private field, writing a constructor parameter, and assigning the field — all in one concise syntax.

```csharp
// Before C# 12
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// After — Primary Constructor (C# 12)
public class OrderService(IOrderRepository repository, ILogger<OrderService> logger)
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Fetching order {Id}", id);
        return await repository.GetByIdAsync(id, ct);
    }
}
```

Combined with **`record`** types (used extensively in the CQRS queries/results of this project) and **collection expressions** (`["EUR", "BRL", "GBP", "AUD"]`), C# 12 makes the code significantly more expressive without sacrificing readability.

---

## 3. How would you track down a performance issue in production?

My approach follows three escalating steps:

**1. Observe first, guess last.**  
I start with dashboards rather than code. In a .NET service that means Application Insights / Datadog / Grafana: request latency percentiles (p50 / p95 / p99), error rates, GC heap pressure, and thread-pool queue depth. The shape of the data tells me whether the problem is CPU-bound, I/O-bound, database, or an external dependency.

**2. Narrow with distributed tracing.**  
OpenTelemetry traces show the waterfall for a single slow request: which span is fat? If the DB span is 2 s but the total is 2.1 s, the DB is the culprit. If every span is fast but total is slow, look at serialization, middleware, or cold-start.

**3. Profile the suspect.**  
For CPU spikes I use `dotnet-trace` / `dotnet-counters` in production (they attach to a running process with zero code changes), then load the resulting `.nettrace` into PerfView or Visual Studio's profiler. For memory issues I take a heap dump with `dotnet-dump` and analyse allocations. For database queries I use EF Core's slow-query log and SQL Server's Query Store.

**Real example:** In a previous project our search API degraded from 80 ms to 1.2 s over a weekend. Traces showed the SQL span growing; Query Store revealed the execution plan had regressed after a statistics update. Rebuilding the index and forcing a plan guide brought latency back under 100 ms immediately.

---

## 4. Latest technical book or conference

The most recent book I finished was **"Building Microservices" (2nd edition) by Sam Newman** (O'Reilly). The key lesson I brought into this project is Newman's emphasis on **defining service boundaries around business capabilities, not data shapes**. That's why I separated the CoinMarketCap concern from the ExchangeRates concern into two independent microservices: they have different external dependencies, different SLAs, and could independently scale or be replaced.

I also attended a session at **.NET Conf 2024** on ASP.NET Core 9 improvements to OpenAPI generation and the new `HybridCache` abstraction, which directly informed the caching improvement I mentioned in Q1.

---

## 5. What do you think about this technical assessment?

It's a well-designed exercise. It tests real-world skills — integrating third-party APIs, handling currency conversion math correctly (including the EUR-base quirk of the free exchangeratesapi tier), structuring a clean solution, and writing meaningful tests — rather than algorithmic puzzles that rarely surface in backend work.

If I were to suggest one improvement it would be making the acceptance criteria slightly more explicit around error cases (e.g. "what should happen for an unknown crypto code?"), because the design of exception handling and user-facing error messages is an important part of a production-quality API.

---

## 6. Describe yourself using JSON

```json
{
  "name": "Senior Backend .NET Developer",
  "core_skills": [".NET / C#", "ASP.NET Core", "Entity Framework Core", "SQL Server", "Docker"],
  "architecture_patterns": ["Clean Architecture", "CQRS", "Microservices", "Domain-Driven Design", "SOLID"],
  "principles": {
    "code_quality": "Readable, testable, minimal — solve the problem, then stop",
    "testing": "Unit tests for logic, integration tests for boundaries, no mocks where a real implementation is cheap",
    "performance": "Measure first, optimise second"
  },
  "soft_skills": {
    "communication": "I write code for the next developer who reads it, not just the compiler",
    "ownership": "I consider a feature 'done' only when it runs correctly in production",
    "learning": "Continuous — currently exploring OpenTelemetry and Aspire"
  },
  "currently_excited_about": ["ASP.NET Core 9 HybridCache", "Vertical Slice Architecture", "Testcontainers for .NET"],
  "outside_work": ["chess", "hiking", "contributing to open-source .NET libraries"]
}
```
