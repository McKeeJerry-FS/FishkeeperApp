# AI Services Developer Guide

**AquaHub - Building Intelligent Features**  
**Last Updated:** February 10, 2026

## Table of Contents

1. [Overview](#overview)
2. [Architecture Pattern](#architecture-pattern)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [Code Examples](#code-examples)
5. [Integration Patterns](#integration-patterns)
6. [Best Practices](#best-practices)
7. [Testing Strategies](#testing-strategies)
8. [Common Pitfalls](#common-pitfalls)

---

## Overview

This guide explains how to implement AI-powered services in AquaHub using the established patterns demonstrated by the `QuarantineCareAdvisorService`. These patterns can be reused for any intelligent feature that analyzes data and provides recommendations.

### What Makes a Service "AI-Powered"?

An AI service in AquaHub:

- **Analyzes** historical data to detect patterns
- **Evaluates** current conditions against optimal ranges
- **Predicts** potential issues before they occur
- **Recommends** specific actions based on context
- **Adapts** advice based on multiple factors
- **Learns** from patterns in the data

### Current AI Services

1. **WaterChemistryPredictionService** - Linear regression predictions
2. **QuarantineCareAdvisorService** - Intelligent care recommendations
3. _Your next service here!_

---

## Architecture Pattern

### The Three-Layer Pattern

```
┌─────────────────────────────────────────┐
│         PRESENTATION LAYER              │
│  (Views/Tank/YourFeature.cshtml)        │
│  - Displays recommendations             │
│  - User interactions                    │
└─────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────┐
│         CONTROLLER LAYER                │
│  (Controllers/TankController.cs)        │
│  - Calls AI service                     │
│  - Prepares view model                  │
│  - Handles user input                   │
└─────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────┐
│         SERVICE LAYER (AI Logic)        │
│  (Services/YourAIService.cs)            │
│  - Data analysis algorithms             │
│  - Pattern recognition                  │
│  - Recommendation generation            │
└─────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────┐
│         DATA LAYER                      │
│  (Models/ + ApplicationDbContext)       │
│  - Entity models                        │
│  - Database access                      │
└─────────────────────────────────────────┘
```

### Key Components

#### 1. Interface (`IYourAIService`)

- Defines the contract
- Enables dependency injection
- Facilitates testing with mocks
- Location: `Services/Interfaces/`

#### 2. Implementation (`YourAIService`)

- Contains actual AI logic
- Business rules and algorithms
- Statistical analysis methods
- Location: `Services/`

#### 3. Result Model (`YourAIRecommendations`)

- Strongly-typed results
- Can be part of interface file or separate
- Should be serializable

#### 4. View Model Integration

- Extends existing view model
- Passes AI results to view
- Location: `Models/ViewModels/`

---

## Step-by-Step Implementation

### Step 1: Define Your AI Service Interface

**File:** `Services/Interfaces/IYourAIService.cs`

```csharp
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

/// <summary>
/// AI service for [describe what it does]
/// </summary>
public interface IYourAIService
{
    /// <summary>
    /// Main analysis method - describes what it analyzes and returns
    /// </summary>
    Task<YourAIRecommendations> AnalyzeAsync(
        Tank tank,
        List<WaterTest> recentTests,
        // Add other data sources needed
        DateTime? referenceDate = null);

    /// <summary>
    /// Helper method for specific analysis sub-task
    /// </summary>
    List<string> AnalyzeSpecificAspect(List<SomeModel> data);
}

/// <summary>
/// Result model containing AI-generated insights
/// </summary>
public class YourAIRecommendations
{
    // Overall assessment
    public string Summary { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical

    // Specific insights grouped by category
    public List<string> KeyInsights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public bool RequiresAction { get; set; }

    // Optional: Structured data for visualizations
    public Dictionary<string, double> Scores { get; set; } = new();
}
```

**Design Tips:**

- Use clear, descriptive names
- Add XML documentation comments
- Return `Task<T>` for async operations
- Group related insights in lists
- Include metadata (timestamp, flags)

---

### Step 2: Implement the Service Logic

**File:** `Services/YourAIService.cs`

```csharp
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

public class YourAIService : IYourAIService
{
    private readonly ILogger<YourAIService> _logger;

    // Inject dependencies you need
    public YourAIService(ILogger<YourAIService> logger)
    {
        _logger = logger;
    }

    public async Task<YourAIRecommendations> AnalyzeAsync(
        Tank tank,
        List<WaterTest> recentTests,
        DateTime? referenceDate = null)
    {
        var recommendations = new YourAIRecommendations();

        try
        {
            // 1. Prepare data
            var orderedData = recentTests
                .OrderBy(t => t.Timestamp)
                .ToList();

            // 2. Perform analysis
            recommendations.KeyInsights = AnalyzeSpecificAspect(orderedData);

            // 3. Calculate risk level
            recommendations.RiskLevel = CalculateRiskLevel(orderedData);

            // 4. Generate recommendations
            recommendations.Recommendations = GenerateActionItems(
                recommendations.RiskLevel,
                orderedData);

            // 5. Create summary
            recommendations.Summary = CreateSummary(recommendations);

            // 6. Set metadata
            recommendations.GeneratedAt = DateTime.Now;
            recommendations.RequiresAction = recommendations.RiskLevel != "Low";

            _logger.LogInformation(
                "Generated AI analysis for tank {TankId}. Risk: {RiskLevel}",
                tank.Id,
                recommendations.RiskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI analysis for tank {TankId}", tank.Id);
            recommendations.Summary = "Unable to generate analysis due to an error.";
            recommendations.RiskLevel = "Unknown";
        }

        return recommendations;
    }

    public List<string> AnalyzeSpecificAspect(List<WaterTest> data)
    {
        var insights = new List<string>();

        if (!data.Any())
        {
            insights.Add("⚠️ No data available for analysis.");
            return insights;
        }

        // Example: Trend detection
        var values = data.Select(d => d.Ammonia ?? 0).ToList();
        var trend = CalculateTrend(values);

        if (trend > 0.01)
        {
            insights.Add("📈 Upward trend detected - parameter is increasing");
        }
        else if (trend < -0.01)
        {
            insights.Add("📉 Downward trend detected - parameter is decreasing");
        }
        else
        {
            insights.Add("➡️ Stable trend - parameter is consistent");
        }

        return insights;
    }

    // Private helper methods for calculations
    private string CalculateRiskLevel(List<WaterTest> data)
    {
        var criticalCount = 0;
        var warningCount = 0;

        var latest = data.LastOrDefault();
        if (latest == null) return "Unknown";

        // Check thresholds
        if (latest.Ammonia > 0.5) criticalCount++;
        else if (latest.Ammonia > 0.25) warningCount++;

        if (latest.Nitrite > 0.5) criticalCount++;
        else if (latest.Nitrite > 0.25) warningCount++;

        // Determine risk
        if (criticalCount > 0) return "Critical";
        if (warningCount >= 2) return "High";
        if (warningCount > 0) return "Medium";
        return "Low";
    }

    private List<string> GenerateActionItems(string riskLevel, List<WaterTest> data)
    {
        var actions = new List<string>();

        switch (riskLevel)
        {
            case "Critical":
                actions.Add("🚨 Perform immediate 50% water change");
                actions.Add("🔬 Test water again in 4 hours");
                actions.Add("💨 Increase aeration");
                break;
            case "High":
                actions.Add("⚠️ Perform 25-30% water change within 24 hours");
                actions.Add("🔬 Test water daily until levels normalize");
                break;
            case "Medium":
                actions.Add("📅 Schedule water change within 2-3 days");
                actions.Add("👁️ Monitor closely for changes");
                break;
            case "Low":
                actions.Add("✅ Continue normal maintenance schedule");
                actions.Add("🔬 Test weekly to maintain stability");
                break;
        }

        return actions;
    }

    private string CreateSummary(YourAIRecommendations recommendations)
    {
        var emoji = recommendations.RiskLevel switch
        {
            "Critical" => "🔴",
            "High" => "🟠",
            "Medium" => "🟡",
            _ => "✅"
        };

        return $"{emoji} {recommendations.RiskLevel} risk level detected. " +
               $"{recommendations.KeyInsights.Count} insights identified. " +
               (recommendations.RequiresAction
                   ? "Action recommended."
                   : "Conditions are optimal.");
    }

    // Statistical helper methods
    private double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;

        var x = Enumerable.Range(0, values.Count).Select(i => (double)i).ToList();
        var y = values;

        var xMean = x.Average();
        var yMean = y.Average();

        var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        var denominator = x.Select(xi => Math.Pow(xi - xMean, 2)).Sum();

        return denominator != 0 ? numerator / denominator : 0;
    }
}
```

**Implementation Tips:**

- Always use try-catch for robustness
- Log analysis events for debugging
- Return graceful fallbacks on errors
- Break complex logic into helper methods
- Use descriptive variable names
- Add comments for complex algorithms

---

### Step 3: Register the Service

**File:** `Program.cs`

```csharp
// Add with other service registrations
builder.Services.AddScoped<IYourAIService, YourAIService>();
```

**Scopes:**

- `AddScoped` - One instance per HTTP request (most common)
- `AddSingleton` - One instance for app lifetime (use for stateless services)
- `AddTransient` - New instance every time (use for lightweight services)

---

### Step 4: Integrate with Controller

**File:** `Controllers/YourController.cs`

```csharp
public class YourController : Controller
{
    private readonly IYourAIService _aiService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<YourController> _logger;

    public YourController(
        IYourAIService aiService,
        ApplicationDbContext context,
        ILogger<YourController> logger)
    {
        _aiService = aiService;
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> DashboardWithAI(int tankId)
    {
        try
        {
            // 1. Fetch required data
            var tank = await _context.Tanks
                .Include(t => t.Livestock)
                .FirstOrDefaultAsync(t => t.Id == tankId);

            if (tank == null)
            {
                return NotFound();
            }

            var recentTests = await _context.WaterTests
                .Where(wt => wt.TankId == tankId
                    && wt.Timestamp >= DateTime.Now.AddDays(-14))
                .OrderBy(wt => wt.Timestamp)
                .ToListAsync();

            // 2. Call AI service
            var aiRecommendations = await _aiService.AnalyzeAsync(
                tank,
                recentTests);

            // 3. Build view model
            var viewModel = new YourViewModel
            {
                Tank = tank,
                RecentTests = recentTests,
                AIRecommendations = aiRecommendations // Add this property to view model
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard for tank {TankId}", tankId);
            TempData["Error"] = "An error occurred loading the dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }
}
```

**Controller Best Practices:**

- Fetch all required data first
- Call AI service with prepared data
- Handle null cases and errors
- Pass results through view model
- Log errors for troubleshooting

---

### Step 5: Update View Model

**File:** `Models/ViewModels/YourViewModel.cs`

```csharp
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Models.ViewModels;

public class YourViewModel
{
    // Existing properties
    public Tank Tank { get; set; } = null!;
    public List<WaterTest> RecentTests { get; set; } = new();

    // Add AI recommendations
    public YourAIRecommendations? AIRecommendations { get; set; }
}
```

---

### Step 6: Display in View

**File:** `Views/Your/Dashboard.cshtml`

```cshtml
@model YourViewModel
@{
    ViewData["Title"] = "AI-Powered Dashboard";
}

<div class="container">
    <!-- Your existing content -->

    <!-- AI Recommendations Section -->
    @if (Model.AIRecommendations != null)
    {
        <div class="row mb-4">
            <div class="col">
                <div class="card border-primary">
                    <div class="card-header bg-primary text-white">
                        <h5 class="mb-0">
                            <i class="bi bi-robot"></i> AI Analysis & Recommendations
                        </h5>
                    </div>
                    <div class="card-body">
                        <!-- Risk Level Alert -->
                        <div class="alert alert-@(GetAlertClass(Model.AIRecommendations.RiskLevel))" role="alert">
                            <h5 class="alert-heading">
                                @GetRiskIcon(Model.AIRecommendations.RiskLevel)
                                Risk Level: @Model.AIRecommendations.RiskLevel
                            </h5>
                            <hr>
                            <p class="mb-0">@Model.AIRecommendations.Summary</p>
                        </div>

                        <!-- Key Insights -->
                        @if (Model.AIRecommendations.KeyInsights.Any())
                        {
                            <div class="card mb-3">
                                <div class="card-header bg-light">
                                    <h6 class="mb-0"><i class="bi bi-lightbulb"></i> Key Insights</h6>
                                </div>
                                <div class="card-body">
                                    <ul class="mb-0">
                                        @foreach (var insight in Model.AIRecommendations.KeyInsights)
                                        {
                                            <li>@insight</li>
                                        }
                                    </ul>
                                </div>
                            </div>
                        }

                        <!-- Recommendations -->
                        @if (Model.AIRecommendations.Recommendations.Any())
                        {
                            <div class="card">
                                <div class="card-header bg-success text-white">
                                    <h6 class="mb-0"><i class="bi bi-list-check"></i> Recommended Actions</h6>
                                </div>
                                <div class="card-body">
                                    <ol class="mb-0">
                                        @foreach (var rec in Model.AIRecommendations.Recommendations)
                                        {
                                            <li>@rec</li>
                                        }
                                    </ol>
                                </div>
                            </div>
                        }

                        <!-- Timestamp -->
                        <div class="text-muted text-end mt-3">
                            <small>
                                <i class="bi bi-clock"></i>
                                Generated: @Model.AIRecommendations.GeneratedAt.ToString("MMM dd, yyyy h:mm tt")
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
</div>

@functions {
    string GetAlertClass(string riskLevel) => riskLevel switch
    {
        "Critical" => "danger",
        "High" => "warning",
        "Medium" => "info",
        _ => "success"
    };

    string GetRiskIcon(string riskLevel) => riskLevel switch
    {
        "Critical" => "<i class='bi bi-exclamation-triangle-fill'></i>",
        "High" => "<i class='bi bi-exclamation-circle-fill'></i>",
        "Medium" => "<i class='bi bi-info-circle-fill'></i>",
        _ => "<i class='bi bi-check-circle-fill'></i>"
    };
}
```

---

## Code Examples

### Example 1: Simple Trend Detector

```csharp
public class SimpleTrendAnalyzer
{
    public string AnalyzeTrend(List<double> values)
    {
        if (values.Count < 3) return "Insufficient data";

        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();

        var change = ((secondHalf - firstHalf) / firstHalf) * 100;

        return change switch
        {
            > 10 => $"📈 Increasing significantly (+{change:F1}%)",
            > 5 => $"↗️ Increasing moderately (+{change:F1}%)",
            < -10 => $"📉 Decreasing significantly ({change:F1}%)",
            < -5 => $"↘️ Decreasing moderately ({change:F1}%)",
            _ => "➡️ Stable"
        };
    }
}
```

### Example 2: Pattern Matcher

```csharp
public class PatternMatcher
{
    public List<string> DetectAnomalies(List<double> values)
    {
        var anomalies = new List<string>();

        if (!values.Any()) return anomalies;

        var mean = values.Average();
        var stdDev = CalculateStdDev(values, mean);

        for (int i = 0; i < values.Count; i++)
        {
            var zScore = Math.Abs((values[i] - mean) / stdDev);

            if (zScore > 2)
            {
                anomalies.Add($"Anomaly detected at position {i}: " +
                    $"value {values[i]:F2} is {zScore:F1} standard deviations from mean");
            }
        }

        return anomalies;
    }

    private double CalculateStdDev(List<double> values, double mean)
    {
        var sumOfSquares = values.Select(v => Math.Pow(v - mean, 2)).Sum();
        return Math.Sqrt(sumOfSquares / values.Count);
    }
}
```

### Example 3: Multi-Factor Risk Calculator

```csharp
public class RiskCalculator
{
    public (string riskLevel, int score) CalculateOverallRisk(
        Dictionary<string, double> factors,
        Dictionary<string, (double warning, double critical)> thresholds)
    {
        int riskScore = 0;

        foreach (var factor in factors)
        {
            var paramName = factor.Key;
            var value = factor.Value;

            if (!thresholds.ContainsKey(paramName)) continue;

            var (warning, critical) = thresholds[paramName];

            if (value >= critical)
            {
                riskScore += 10;
            }
            else if (value >= warning)
            {
                riskScore += 5;
            }
        }

        var riskLevel = riskScore switch
        {
            >= 20 => "Critical",
            >= 10 => "High",
            >= 5 => "Medium",
            _ => "Low"
        };

        return (riskLevel, riskScore);
    }
}
```

---

## Integration Patterns

### Pattern 1: Service Composition

Combine multiple AI services:

```csharp
public class CompositeAnalysisService
{
    private readonly IWaterChemistryPredictionService _predictionService;
    private readonly IQuarantineCareAdvisorService _careAdvisor;
    private readonly IYourAIService _yourService;

    public async Task<CompleteAnalysis> GetCompleteAnalysisAsync(Tank tank)
    {
        // Run analyses in parallel
        var predictionTask = _predictionService.PredictAsync(tank.Id);
        var careTask = _careAdvisor.AnalyzeAsync(tank, ...);
        var yourTask = _yourService.AnalyzeAsync(tank, ...);

        await Task.WhenAll(predictionTask, careTask, yourTask);

        return new CompleteAnalysis
        {
            Predictions = await predictionTask,
            CareAdvice = await careTask,
            YourAnalysis = await yourTask
        };
    }
}
```

### Pattern 2: Caching Results

For expensive calculations:

```csharp
public class CachedAIService : IYourAIService
{
    private readonly IMemoryCache _cache;
    private readonly IYourAIService _innerService;

    public async Task<YourAIRecommendations> AnalyzeAsync(...)
    {
        var cacheKey = $"ai_analysis_{tank.Id}_{DateTime.Now:yyyyMMddHH}";

        if (_cache.TryGetValue(cacheKey, out YourAIRecommendations cached))
        {
            return cached;
        }

        var result = await _innerService.AnalyzeAsync(...);

        _cache.Set(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }
}
```

### Pattern 3: Background Processing

For long-running analysis:

```csharp
public class BackgroundAnalysisService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var aiService = scope.ServiceProvider.GetRequiredService<IYourAIService>();

            // Analyze all tanks
            var tanks = await GetAllActiveTanks();

            foreach (var tank in tanks)
            {
                await aiService.AnalyzeAsync(tank, ...);
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

---

## Best Practices

### 1. Data Quality

```csharp
// Always validate input data
private bool HasSufficientData(List<WaterTest> tests)
{
    return tests.Count >= 3 &&
           tests.Any(t => t.Timestamp >= DateTime.Now.AddDays(-7));
}

// Handle missing data gracefully
var ammoniaValues = tests
    .Where(t => t.Ammonia.HasValue)
    .Select(t => t.Ammonia!.Value)
    .ToList();

if (!ammoniaValues.Any())
{
    return "Insufficient ammonia data for analysis";
}
```

### 2. Performance Optimization

```csharp
// Use async/await properly
public async Task<YourAIRecommendations> AnalyzeAsync(...)
{
    // Run independent analyses in parallel
    var task1 = AnalyzeAspect1Async(data1);
    var task2 = AnalyzeAspect2Async(data2);
    var task3 = AnalyzeAspect3Async(data3);

    await Task.WhenAll(task1, task2, task3);

    return CombineResults(
        await task1,
        await task2,
        await task3);
}

// Use efficient LINQ
// ❌ Bad: Multiple iterations
var count = data.Count();
var sum = data.Sum(x => x.Value);
var avg = data.Average(x => x.Value);

// ✅ Good: Single iteration
var (count, sum, avg) = data.Aggregate(
    (count: 0, sum: 0.0, avg: 0.0),
    (acc, item) => (acc.count + 1, acc.sum + item.Value, acc.avg),
    acc => (acc.count, acc.sum, acc.sum / acc.count));
```

### 3. Error Handling

```csharp
public async Task<YourAIRecommendations> AnalyzeAsync(...)
{
    var recommendations = new YourAIRecommendations();

    try
    {
        // Main analysis logic
        recommendations = await PerformAnalysisAsync(...);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Invalid operation during analysis");
        recommendations.Summary = "Unable to complete analysis - invalid data";
        recommendations.RiskLevel = "Unknown";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during analysis");
        recommendations.Summary = "Analysis failed - please try again";
        recommendations.RiskLevel = "Unknown";
    }

    return recommendations;
}
```

### 4. Testability

```csharp
// Make algorithms testable
public class YourAIService : IYourAIService
{
    // Public interface method
    public async Task<YourAIRecommendations> AnalyzeAsync(...)
    {
        var trend = CalculateTrend(data);
        return BuildRecommendations(trend);
    }

    // Internal methods are testable
    internal double CalculateTrend(List<double> values)
    {
        // Pure function - easy to unit test
        if (values.Count < 2) return 0;
        // ... calculation logic
    }

    internal YourAIRecommendations BuildRecommendations(double trend)
    {
        // Testable without async complexity
        return new YourAIRecommendations
        {
            Summary = trend > 0 ? "Increasing" : "Stable"
        };
    }
}
```

### 5. Configuration

```csharp
// Use configuration for thresholds
public class AIServiceOptions
{
    public double AmmoniaWarningLevel { get; set; } = 0.25;
    public double AmmoniaCriticalLevel { get; set; } = 0.5;
    public int MinDataPoints { get; set; } = 3;
    public int AnalysisWindowDays { get; set; } = 14;
}

// In Program.cs
builder.Services.Configure<AIServiceOptions>(
    builder.Configuration.GetSection("AIService"));

// In service
public class YourAIService : IYourAIService
{
    private readonly AIServiceOptions _options;

    public YourAIService(IOptions<AIServiceOptions> options)
    {
        _options = options.Value;
    }

    private bool IsWarningLevel(double ammonia)
    {
        return ammonia > _options.AmmoniaWarningLevel;
    }
}
```

---

## Testing Strategies

### Unit Testing

```csharp
public class YourAIServiceTests
{
    [Fact]
    public void CalculateTrend_WithIncreasingValues_ReturnsPositiveTrend()
    {
        // Arrange
        var service = new YourAIService(Mock.Of<ILogger<YourAIService>>());
        var values = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var result = service.CalculateTrend(values);

        // Assert
        Assert.True(result > 0, "Trend should be positive for increasing values");
        Assert.InRange(result, 0.9, 1.1); // Linear increase = slope of 1
    }

    [Fact]
    public async Task AnalyzeAsync_WithNoData_ReturnsUnknownRisk()
    {
        // Arrange
        var service = new YourAIService(Mock.Of<ILogger<YourAIService>>());
        var tank = new Tank { Id = 1 };
        var emptyTests = new List<WaterTest>();

        // Act
        var result = await service.AnalyzeAsync(tank, emptyTests);

        // Assert
        Assert.Equal("Unknown", result.RiskLevel);
        Assert.Contains("No data", result.Summary);
    }

    [Theory]
    [InlineData(0.1, "Low")]
    [InlineData(0.3, "Medium")]
    [InlineData(0.6, "Critical")]
    public void CalculateRiskLevel_WithDifferentAmmonia_ReturnsCorrectRisk(
        double ammonia,
        string expectedRisk)
    {
        // Arrange
        var service = new YourAIService(Mock.Of<ILogger<YourAIService>>());
        var tests = new List<WaterTest>
        {
            new WaterTest { Ammonia = ammonia }
        };

        // Act
        var result = service.CalculateRiskLevel(tests);

        // Assert
        Assert.Equal(expectedRisk, result);
    }
}
```

### Integration Testing

```csharp
public class AIServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AIServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AIService_WithRealData_GeneratesRecommendations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IYourAIService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tank = new Tank { Name = "Test Tank" };
        context.Tanks.Add(tank);
        await context.SaveChangesAsync();

        var tests = Enumerable.Range(1, 7)
            .Select(i => new WaterTest
            {
                TankId = tank.Id,
                Ammonia = 0.1 * i,
                Timestamp = DateTime.Now.AddDays(-i)
            })
            .ToList();

        context.WaterTests.AddRange(tests);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AnalyzeAsync(tank, tests);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Summary);
        Assert.Contains(result.RiskLevel, new[] { "Low", "Medium", "High", "Critical" });
    }
}
```

---

## Common Pitfalls

### ❌ Pitfall 1: Synchronous Operations

```csharp
// BAD - Blocks the thread
public YourAIRecommendations Analyze(Tank tank)
{
    var data = _context.WaterTests.Where(...).ToList(); // Sync DB call
    return ProcessData(data);
}

// GOOD - Async all the way
public async Task<YourAIRecommendations> AnalyzeAsync(Tank tank)
{
    var data = await _context.WaterTests.Where(...).ToListAsync();
    return ProcessData(data);
}
```

### ❌ Pitfall 2: Division by Zero

```csharp
// BAD - Can throw exception
var average = sum / count;

// GOOD - Handle zero case
var average = count > 0 ? sum / count : 0;
```

### ❌ Pitfall 3: Not Handling Nulls

```csharp
// BAD - NullReferenceException
var ammonia = test.Ammonia.Value; // What if Ammonia is null?

// GOOD - Safe access
var ammonia = test.Ammonia ?? 0;
// or
if (!test.Ammonia.HasValue) continue;
```

### ❌ Pitfall 4: Magic Numbers

```csharp
// BAD - What does 0.25 mean?
if (ammonia > 0.25)

// GOOD - Named constants
private const double AMMONIA_WARNING_THRESHOLD = 0.25;
if (ammonia > AMMONIA_WARNING_THRESHOLD)
```

### ❌ Pitfall 5: Ignoring Time Zones

```csharp
// BAD - UTC vs Local confusion
var today = DateTime.Now.Date;
var recentTests = tests.Where(t => t.Timestamp >= today);

// GOOD - Be explicit
var today = DateTime.UtcNow.Date;
var recentTests = tests.Where(t => t.Timestamp.Date >= today);
```

---

## Real-World Example: Implementing a New Feature

Let's implement a **Coral Growth AI Advisor**:

### Requirements

- Analyze coral growth patterns
- Detect stunted growth
- Recommend lighting/nutrient adjustments
- Predict growth rates

### Implementation

**1. Interface:**

```csharp
public interface ICoralGrowthAdvisorService
{
    Task<CoralGrowthRecommendations> AnalyzeCoralGrowthAsync(
        int livestockId,
        List<GrowthRecord> growthRecords,
        List<WaterTest> waterTests);
}

public class CoralGrowthRecommendations
{
    public string GrowthStatus { get; set; } = string.Empty; // Excellent, Good, Slow, Stunted
    public double AverageGrowthRate { get; set; } // cm per month
    public double PredictedGrowthNextMonth { get; set; }
    public List<string> LightingRecommendations { get; set; } = new();
    public List<string> NutrientRecommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
```

**2. Service:**

```csharp
public class CoralGrowthAdvisorService : ICoralGrowthAdvisorService
{
    public async Task<CoralGrowthRecommendations> AnalyzeCoralGrowthAsync(
        int livestockId,
        List<GrowthRecord> growthRecords,
        List<WaterTest> waterTests)
    {
        var recommendations = new CoralGrowthRecommendations();

        // Calculate growth rate
        var orderedRecords = growthRecords.OrderBy(r => r.DateRecorded).ToList();
        var growthRate = CalculateGrowthRate(orderedRecords);
        recommendations.AverageGrowthRate = growthRate;

        // Determine status
        recommendations.GrowthStatus = growthRate switch
        {
            >= 1.5 => "Excellent",
            >= 1.0 => "Good",
            >= 0.5 => "Slow",
            _ => "Stunted"
        };

        // Analyze water chemistry impact
        var avgCalcium = waterTests.Average(t => t.Calcium ?? 400);
        var avgAlkalinity = waterTests.Average(t => t.Alkalinity ?? 8);

        if (avgCalcium < 380)
        {
            recommendations.NutrientRecommendations.Add(
                "⚠️ Low calcium detected. Target: 400-450 ppm");
        }

        if (avgAlkalinity < 8)
        {
            recommendations.NutrientRecommendations.Add(
                "⚠️ Low alkalinity detected. Target: 8-12 dKH");
        }

        // Predict next month
        recommendations.PredictedGrowthNextMonth = PredictGrowth(growthRate, waterTests);

        return recommendations;
    }

    private double CalculateGrowthRate(List<GrowthRecord> records)
    {
        if (records.Count < 2) return 0;

        var first = records.First();
        var last = records.Last();

        var daysDiff = (last.DateRecorded - first.DateRecorded).TotalDays;
        var sizeDiff = last.Size - first.Size;

        return (sizeDiff / daysDiff) * 30; // Convert to per month
    }

    private double PredictGrowth(double currentRate, List<WaterTest> waterTests)
    {
        // Simple prediction: current rate adjusted by water quality
        var qualityFactor = CalculateWaterQualityFactor(waterTests);
        return currentRate * qualityFactor;
    }

    private double CalculateWaterQualityFactor(List<WaterTest> waterTests)
    {
        var latest = waterTests.LastOrDefault();
        if (latest == null) return 1.0;

        var factor = 1.0;

        // Adjust based on parameters
        if (latest.Calcium < 380) factor *= 0.8;
        if (latest.Alkalinity < 8) factor *= 0.9;
        if (latest.Nitrate > 20) factor *= 0.95;

        return factor;
    }
}
```

**3. Register:**

```csharp
builder.Services.AddScoped<ICoralGrowthAdvisorService, CoralGrowthAdvisorService>();
```

**4. Use in Controller:**

```csharp
public async Task<IActionResult> CoralDetails(int id)
{
    var coral = await _context.Livestock.FindAsync(id);
    var growthRecords = await _context.GrowthRecords
        .Where(g => g.LivestockId == id)
        .ToListAsync();
    var waterTests = await _context.WaterTests
        .Where(w => w.TankId == coral.TankId)
        .OrderByDescending(w => w.Timestamp)
        .Take(10)
        .ToListAsync();

    var aiRecommendations = await _coralGrowthAdvisor.AnalyzeCoralGrowthAsync(
        id, growthRecords, waterTests);

    var viewModel = new CoralDetailsViewModel
    {
        Coral = coral,
        GrowthRecords = growthRecords,
        AIRecommendations = aiRecommendations
    };

    return View(viewModel);
}
```

---

## Conclusion

You now have a complete template for implementing AI-powered services in AquaHub:

1. ✅ **Define** clear interfaces with result models
2. ✅ **Implement** analysis logic with proper error handling
3. ✅ **Register** services for dependency injection
4. ✅ **Integrate** with controllers and views
5. ✅ **Test** thoroughly with unit and integration tests
6. ✅ **Document** your AI logic for future maintainers

### Key Takeaways

- Keep AI logic separate from controllers (Service Layer pattern)
- Use interfaces for testability and flexibility
- Handle errors gracefully with fallback responses
- Log analysis events for debugging
- Return structured, actionable recommendations
- Use async/await for all I/O operations
- Validate input data before analysis
- Make thresholds configurable

### Next Steps

1. Identify a feature that would benefit from AI analysis
2. Define what insights you want to provide
3. Follow this guide to implement your service
4. Test thoroughly with real data
5. Iterate based on user feedback

**Happy building! 🚀**

---

_For questions or contributions, see the main [README.md](../README.md)_
