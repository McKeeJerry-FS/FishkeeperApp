# AI Services Quick Reference Card

**AquaHub - AI Implementation Cheat Sheet**

## 📋 Quick Setup Checklist

- [ ] Create interface in `Services/Interfaces/I[YourService].cs`
- [ ] Create result model class in same file
- [ ] Implement service in `Services/[YourService].cs`
- [ ] Register in `Program.cs`: `builder.Services.AddScoped<IYourService, YourService>()`
- [ ] Add property to view model: `public YourRecommendations? AIResults { get; set; }`
- [ ] Inject service into controller constructor
- [ ] Call service in action method
- [ ] Display results in view with card UI
- [ ] Write unit tests
- [ ] Document thresholds and logic

---

## 🎯 Interface Template

```csharp
public interface IYourAIService
{
    Task<YourRecommendations> AnalyzeAsync([params]);
}

public class YourRecommendations
{
    public string Summary { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low";
    public List<string> Insights { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
```

---

## 🔧 Service Template

```csharp
public class YourAIService : IYourAIService
{
    private readonly ILogger<YourAIService> _logger;

    public YourAIService(ILogger<YourAIService> logger)
    {
        _logger = logger;
    }

    public async Task<YourRecommendations> AnalyzeAsync([params])
    {
        var result = new YourRecommendations();

        try
        {
            // 1. Validate data
            // 2. Analyze
            // 3. Generate recommendations
            // 4. Calculate risk

            _logger.LogInformation("Analysis complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed");
            result.Summary = "Analysis failed";
        }

        return result;
    }
}
```

---

## 🎨 View Template

```cshtml
@if (Model.AIResults != null)
{
    <div class="card border-primary">
        <div class="card-header bg-primary text-white">
            <h5><i class="bi bi-robot"></i> AI Analysis</h5>
        </div>
        <div class="card-body">
            <div class="alert alert-@GetAlertClass(Model.AIResults.RiskLevel)">
                <h5>Risk: @Model.AIResults.RiskLevel</h5>
                <p>@Model.AIResults.Summary</p>
            </div>

            @if (Model.AIResults.Insights.Any())
            {
                <h6>Insights:</h6>
                <ul>
                    @foreach (var insight in Model.AIResults.Insights)
                    {
                        <li>@insight</li>
                    }
                </ul>
            }
        </div>
    </div>
}

@functions {
    string GetAlertClass(string risk) => risk switch
    {
        "Critical" => "danger",
        "High" => "warning",
        "Medium" => "info",
        _ => "success"
    };
}
```

---

## 📊 Common Algorithms

### Trend Detection

```csharp
private double CalculateTrend(List<double> values)
{
    if (values.Count < 2) return 0;

    var x = Enumerable.Range(0, values.Count).Select(i => (double)i).ToList();
    var xMean = x.Average();
    var yMean = values.Average();

    var numerator = x.Zip(values, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
    var denominator = x.Select(xi => Math.Pow(xi - xMean, 2)).Sum();

    return denominator != 0 ? numerator / denominator : 0;
}
```

### Standard Deviation

```csharp
private double CalculateStdDev(List<double> values)
{
    var mean = values.Average();
    var sumOfSquares = values.Select(v => Math.Pow(v - mean, 2)).Sum();
    return Math.Sqrt(sumOfSquares / values.Count);
}
```

### Simple Moving Average

```csharp
private List<double> CalculateSMA(List<double> values, int period)
{
    return values
        .Select((_, i) => values.Skip(Math.Max(0, i - period + 1)).Take(period).Average())
        .ToList();
}
```

---

## 🎭 Risk Level Guidelines

| Risk         | Conditions                      | Action                                  |
| ------------ | ------------------------------- | --------------------------------------- |
| **Critical** | Toxic levels, multiple failures | Immediate action required               |
| **High**     | 2+ warning parameters           | Action within 24h                       |
| **Medium**   | 1 warning parameter             | Monitor closely, action within 2-3 days |
| **Low**      | All optimal                     | Continue normal routine                 |

---

## 🔍 Data Validation Patterns

```csharp
// Check minimum data points
if (data.Count < 3)
{
    return "Insufficient data (minimum 3 required)";
}

// Check recency
var daysSinceLastTest = (DateTime.Now - data.Last().Timestamp).Days;
if (daysSinceLastTest > 7)
{
    warnings.Add("Data is over 7 days old");
}

// Handle nulls
var validValues = data
    .Where(d => d.Value.HasValue)
    .Select(d => d.Value!.Value)
    .ToList();

if (!validValues.Any())
{
    return "No valid values found";
}
```

---

## ⚡ Performance Tips

```csharp
// ✅ DO: Use async/await
public async Task<Results> AnalyzeAsync()
{
    var data = await _context.Data.ToListAsync();
    return Process(data);
}

// ✅ DO: Run independent tasks in parallel
var task1 = AnalyzeAspect1Async();
var task2 = AnalyzeAspect2Async();
await Task.WhenAll(task1, task2);

// ✅ DO: Filter in database
var data = await _context.Tests
    .Where(t => t.Date >= cutoffDate)
    .ToListAsync();

// ❌ DON'T: Block async code
var result = AnalyzeAsync().Result; // Blocks thread!

// ❌ DON'T: Multiple enumerations
var count = data.Count();
var sum = data.Sum(); // Enumerates again!
```

---

## 🧪 Unit Test Template

```csharp
[Fact]
public async Task AnalyzeAsync_WithValidData_ReturnsRecommendations()
{
    // Arrange
    var logger = Mock.Of<ILogger<YourService>>();
    var service = new YourService(logger);
    var testData = new List<DataPoint>
    {
        new() { Value = 1.0, Date = DateTime.Now.AddDays(-2) },
        new() { Value = 2.0, Date = DateTime.Now.AddDays(-1) },
        new() { Value = 3.0, Date = DateTime.Now }
    };

    // Act
    var result = await service.AnalyzeAsync(testData);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Insights);
    Assert.Contains(result.RiskLevel, new[] { "Low", "Medium", "High", "Critical" });
}

[Theory]
[InlineData(0.1, "Low")]
[InlineData(0.3, "Medium")]
[InlineData(0.6, "High")]
public void CalculateRisk_WithDifferentValues_ReturnsExpectedRisk(
    double value,
    string expectedRisk)
{
    var service = new YourService(Mock.Of<ILogger<YourService>>());
    var result = service.CalculateRisk(value);
    Assert.Equal(expectedRisk, result);
}
```

---

## 📝 Common Patterns

### Pattern 1: Threshold-Based Analysis

```csharp
var risk = value switch
{
    >= CRITICAL_THRESHOLD => "Critical",
    >= WARNING_THRESHOLD => "High",
    >= CAUTION_THRESHOLD => "Medium",
    _ => "Low"
};
```

### Pattern 2: Trend + Threshold

```csharp
var trend = CalculateTrend(values);
var current = values.Last();

if (current > CRITICAL_THRESHOLD && trend > 0)
{
    return "Critical - Value high and rising";
}
else if (current > WARNING_THRESHOLD)
{
    return trend > 0 ? "Warning - Rising trend" : "Caution - High but stable";
}
```

### Pattern 3: Multi-Factor Score

```csharp
var score = 0;

if (ammonia > 0.25) score += 10;
if (nitrite > 0.25) score += 10;
if (nitrate > 40) score += 5;
if (ph < 7.8 || ph > 8.4) score += 3;

var risk = score switch
{
    >= 20 => "Critical",
    >= 10 => "High",
    >= 5 => "Medium",
    _ => "Low"
};
```

---

## 🚫 Common Mistakes

| ❌ Mistake                    | ✅ Fix                                                  |
| ----------------------------- | ------------------------------------------------------- |
| `var avg = sum / count;`      | `var avg = count > 0 ? sum / count : 0;`                |
| `var value = nullable.Value;` | `var value = nullable ?? 0;`                            |
| `if (value > 0.25)`           | `const double THRESHOLD = 0.25; if (value > THRESHOLD)` |
| `DateTime.Now` everywhere     | `DateTime.UtcNow` or be consistent                      |
| Sync DB calls                 | `await _context.Data.ToListAsync()`                     |
| No error handling             | Wrap in try-catch with logging                          |
| Magic numbers                 | Named constants or configuration                        |

---

## 🎯 AI Service Scopes

Choose the right scope in `Program.cs`:

```csharp
// Most common - one instance per HTTP request
builder.Services.AddScoped<IYourService, YourService>();

// One instance for app lifetime - use for stateless services
builder.Services.AddSingleton<IYourService, YourService>();

// New instance every time - use for lightweight operations
builder.Services.AddTransient<IYourService, YourService>();
```

---

## 📚 Related Files

| File                                                                           | Purpose                      |
| ------------------------------------------------------------------------------ | ---------------------------- |
| [AI_SERVICES_DEVELOPER_GUIDE.md](AI_SERVICES_DEVELOPER_GUIDE.md)               | Full implementation guide    |
| [AI_QUARANTINE_CARE_GUIDE.md](AI_QUARANTINE_CARE_GUIDE.md)                     | User guide for quarantine AI |
| [MACHINE_LEARNING_PREDICTIONS_GUIDE.md](MACHINE_LEARNING_PREDICTIONS_GUIDE.md) | ML concepts and algorithms   |

---

## 🔗 Quick Links

- **Interface Location:** `Services/Interfaces/`
- **Service Location:** `Services/`
- **View Models:** `Models/ViewModels/`
- **Registration:** `Program.cs`
- **Tests:** Create in test project

---

**Pro Tip:** Copy this reference card and the developer guide - they contain everything you need to implement AI features! 🚀
