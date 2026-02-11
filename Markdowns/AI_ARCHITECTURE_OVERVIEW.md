# AI Services Architecture Overview

**Visual Guide to AquaHub's AI Implementation**

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                           USER INTERFACE                            │
│                    (Browser / Mobile Device)                        │
└─────────────────────────────────────────────────────────────────────┘
                                   ↕ HTTP/HTTPS
┌─────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                             │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  │ QuarantineDash.  │  │ Prediction       │  │ Tank             │ │
│  │ cshtml           │  │ Dashboard.cshtml │  │ Dashboard.cshtml │ │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘ │
│          ↓                       ↓                       ↓          │
└─────────────────────────────────────────────────────────────────────┘
                                   ↕ Razor View Models
┌─────────────────────────────────────────────────────────────────────┐
│                       CONTROLLER LAYER                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  │ TankController   │  │ PredictionCtrl   │  │ OtherControllers │ │
│  │ - QuarantineDash │  │ - Dashboard      │  │ - Various        │ │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘ │
│          ↓                       ↓                       ↓          │
└─────────────────────────────────────────────────────────────────────┘
                                   ↕ Dependency Injection
┌─────────────────────────────────────────────────────────────────────┐
│                      AI SERVICE LAYER                               │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ IQuarantineCareAdvisorService                               │   │
│  │  ├── AnalyzeQuarantineConditionsAsync()                     │   │
│  │  ├── AnalyzeWaterChemistryTrends()                          │   │
│  │  ├── EvaluateDosingProtocol()                               │   │
│  │  ├── GetDurationBasedRecommendations()                      │   │
│  │  └── SuggestNextSteps()                                     │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ IWaterChemistryPredictionService                            │   │
│  │  ├── PredictParametersAsync()                               │   │
│  │  ├── CalculateLinearRegression()                            │   │
│  │  ├── CalculateRSquared()                                    │   │
│  │  └── PredictSingleParameter()                               │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ Your Future AI Service                                      │   │
│  │  └── Implement following the same pattern                   │   │
│  └─────────────────────────────────────────────────────────────┘   │
│          ↓                       ↓                       ↓          │
└─────────────────────────────────────────────────────────────────────┘
                                   ↕ Entity Framework Core
┌─────────────────────────────────────────────────────────────────────┐
│                      DATA ACCESS LAYER                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐ │
│  │ ApplicationDb    │  │ Tank Service     │  │ Other Services   │ │
│  │ Context          │  │ Livestock Svc    │  │ Equipment Svc    │ │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘ │
│          ↓                       ↓                       ↓          │
└─────────────────────────────────────────────────────────────────────┘
                                   ↕ SQL Queries
┌─────────────────────────────────────────────────────────────────────┐
│                         DATABASE                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌───────────┐ │
│  │ Tanks        │ │ WaterTests   │ │ DosingRecords│ │ Livestock │ │
│  └──────────────┘ └──────────────┘ └──────────────┘ └───────────┘ │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌───────────┐ │
│  │ Maintenance  │ │ Feeding      │ │ GrowthRecords│ │ Equipment │ │
│  └──────────────┘ └──────────────┘ └──────────────┘ └───────────┘ │
│                    PostgreSQL / SQLite                              │
└─────────────────────────────────────────────────────────────────────┘
```

---

## AI Service Interaction Flow

### Example: Quarantine Care Analysis Request

```
1. USER ACTION
   User navigates to Quarantine Dashboard
   └─> GET /Tank/QuarantineDashboard/5

2. CONTROLLER (TankController.cs)
   ┌─────────────────────────────────────────┐
   │ QuarantineDashboard(int id)             │
   │ ├─ Load tank from database              │
   │ ├─ Load recent water tests (14 days)    │
   │ ├─ Load dosing records                  │
   │ ├─ Load maintenance logs                │
   │ ├─ Load quarantined livestock           │
   │ └─ Call AI service ─────────────────────┼──┐
   └─────────────────────────────────────────┘  │
                                                 │
3. AI SERVICE (QuarantineCareAdvisorService.cs) │
   ┌──────────────────────────────────────────<─┘
   │ AnalyzeQuarantineConditionsAsync()
   │
   ├─[1] WATER CHEMISTRY ANALYSIS
   │     ├─ Calculate trends (linear regression)
   │     ├─ Detect rising/falling parameters
   │     ├─ Check pH stability (std deviation)
   │     ├─ Compare against thresholds
   │     └─ Generate insights ────────────────┐
   │                                           │
   ├─[2] DOSING PROTOCOL EVALUATION           │
   │     ├─ Identify medication types         │
   │     ├─ Match against protocol database   │
   │     ├─ Calculate treatment duration      │
   │     ├─ Check dosing frequency            │
   │     └─ Generate recommendations ─────────┤
   │                                           │
   ├─[3] DURATION-BASED ADVICE                │
   │     ├─ Calculate days in quarantine      │
   │     ├─ Determine phase (Early/Mid/Late)  │
   │     ├─ Adjust recommendations by phase   │
   │     └─ Generate timeline guidance ───────┤
   │                                           │
   ├─[4] RISK ASSESSMENT                      │
   │     ├─ Count critical parameters         │
   │     ├─ Count warning parameters          │
   │     ├─ Calculate risk score              │
   │     ├─ Determine risk level              │
   │     └─ Set urgency flags ────────────────┤
   │                                           │
   └─[5] CONSOLIDATE RESULTS                  │
         ├─ Create summary message            │
         ├─ Compile all insights      <───────┘
         ├─ Prioritize recommendations
         ├─ Add metadata (timestamp)
         └─ Return QuarantineCareRecommendations
                         │
4. BACK TO CONTROLLER    │
   ┌─────────────────────┘
   │ Receive AIRecommendations
   │ ├─ Add to view model
   │ └─ Pass to view
   │
5. VIEW (QuarantineDashboard.cshtml)
   ┌─────────────────────────────────────────┐
   │ Display AI Recommendations Card         │
   │ ├─ Risk level alert (color-coded)       │
   │ ├─ Water chemistry insights             │
   │ ├─ Dosing recommendations               │
   │ ├─ Maintenance actions                  │
   │ ├─ Monitoring priorities                │
   │ └─ Next steps                            │
   └─────────────────────────────────────────┘
                         │
6. USER SEES RESULTS     │
   ┌─────────────────────┘
   │ Beautiful AI-powered dashboard
   │ ├─ Clear risk assessment
   │ ├─ Actionable recommendations
   │ └─ Educational insights
   └─> User takes informed action
```

---

## Data Flow Diagram

```
┌──────────────┐
│   DATABASE   │
│              │
│ • Tanks      │
│ • WaterTests │
│ • Dosing     │
│ • Maintenance│
└──────┬───────┘
       │ EF Core Query
       ↓
┌──────────────────────┐
│   CONTROLLER         │
│                      │
│ Fetches Data:        │
│ • Last 14 days tests │
│ • Recent dosing      │
│ • Maintenance logs   │
└──────┬───────────────┘
       │ Pass to Service
       ↓
┌────────────────────────────────────────┐
│        AI SERVICE                      │
│                                        │
│ ┌────────────────────────────────┐    │
│ │ Input Data Processing          │    │
│ │ • Sort by timestamp            │    │
│ │ • Filter nulls                 │    │
│ │ • Group by type                │    │
│ └──────────┬─────────────────────┘    │
│            ↓                           │
│ ┌────────────────────────────────┐    │
│ │ Analysis Algorithms            │    │
│ │ • Linear regression            │    │
│ │ • Standard deviation           │    │
│ │ • Pattern matching             │    │
│ │ • Threshold comparison         │    │
│ └──────────┬─────────────────────┘    │
│            ↓                           │
│ ┌────────────────────────────────┐    │
│ │ Recommendation Engine          │    │
│ │ • Risk calculation             │    │
│ │ • Insight generation           │    │
│ │ • Action prioritization        │    │
│ └──────────┬─────────────────────┘    │
│            ↓                           │
│ ┌────────────────────────────────┐    │
│ │ Output: Recommendations Object │    │
│ │ • Summary                      │    │
│ │ • Risk level                   │    │
│ │ • Insights list                │    │
│ │ • Actions list                 │    │
│ └──────────┬─────────────────────┘    │
└────────────┼────────────────────────────┘
             │ Return Results
             ↓
┌────────────────────────┐
│   CONTROLLER           │
│ • Receives results     │
│ • Adds to view model   │
└──────┬─────────────────┘
       │ Pass to View
       ↓
┌────────────────────────┐
│   VIEW (Razor)         │
│ • Render HTML          │
│ • Display insights     │
│ • Show recommendations │
└──────┬─────────────────┘
       │ HTTP Response
       ↓
┌────────────────────────┐
│   USER'S BROWSER       │
│ • Beautiful UI         │
│ • Interactive cards    │
│ • Actionable info      │
└────────────────────────┘
```

---

## Service Dependency Graph

```
Program.cs (Startup)
    │
    ├─ Register Services (Dependency Injection)
    │  │
    │  ├─ AddScoped<IQuarantineCareAdvisorService, QuarantineCareAdvisorService>
    │  │      │
    │  │      └─ Dependencies:
    │  │         └─ ILogger<QuarantineCareAdvisorService>
    │  │
    │  ├─ AddScoped<IWaterChemistryPredictionService, WaterChemistryPredictionService>
    │  │      │
    │  │      └─ Dependencies:
    │  │         └─ ILogger<WaterChemistryPredictionService>
    │  │
    │  └─ AddScoped<ITankService, TankService>
    │         │
    │         └─ Dependencies:
    │            ├─ ApplicationDbContext
    │            ├─ IImageService
    │            └─ UserManager<AppUser>
    │
    └─ Controllers Use Services
       │
       ├─ TankController
       │  ├─ Injects: IQuarantineCareAdvisorService
       │  ├─ Injects: ITankService
       │  ├─ Injects: ApplicationDbContext
       │  └─ Uses in: QuarantineDashboard() action
       │
       └─ PredictionController
          ├─ Injects: IWaterChemistryPredictionService
          └─ Uses in: Dashboard() action
```

---

## Algorithmic Pipeline

### Quarantine Care Analysis Pipeline

```
START
  │
  ├─[Input Validation]
  │  ├─ Check tank exists ────────> If no: Return error
  │  ├─ Check has water tests ────> If no: Return "no data" message
  │  └─ Check data recency ───────> If old: Add warning
  │
  ├─[Data Preparation]
  │  ├─ Sort by timestamp (ascending)
  │  ├─ Filter out nulls
  │  ├─ Group by parameter type
  │  └─ Calculate derived values (days in quarantine, etc.)
  │
  ├─[Trend Analysis] ──────────────────────┐
  │  ├─ For each parameter:                │
  │  │  ├─ Extract time series             │
  │  │  ├─ Calculate linear regression     │
  │  │  │  └─ Slope = trend direction      │
  │  │  ├─ Classify: Rising/Falling/Stable │
  │  │  └─ Store insight                   │
  │  └─ Calculate pH stability (std dev)   │
  │                                         │
  ├─[Threshold Comparison] ────────────────┤
  │  ├─ Latest values vs thresholds        │
  │  │  ├─ Ammonia > 0.5? → Critical       │
  │  │  ├─ Ammonia > 0.25? → Warning       │
  │  │  ├─ Nitrite > 0.5? → Critical       │
  │  │  ├─ Nitrite > 0.25? → Warning       │
  │  │  └─ ... (other parameters)          │
  │  └─ Generate parameter-specific alerts │
  │                                         │
  ├─[Pattern Recognition] ─────────────────┤
  │  ├─ Scan dosing records                │
  │  ├─ Match medication names             │
  │  │  ├─ Contains "copper"? → Copper protocol
  │  │  ├─ Contains "prazi"? → Praziquantel protocol
  │  │  ├─ Contains "metro"? → Metronidazole protocol
  │  │  └─ Contains "kana"? → Kanamycin protocol
  │  └─ Generate medication-specific advice│
  │                                         │
  ├─[Contextual Analysis] ─────────────────┤
  │  ├─ Calculate days in quarantine       │
  │  ├─ Determine phase:                   │
  │  │  ├─ Days 1-7 → Early phase          │
  │  │  ├─ Days 8-21 → Mid phase           │
  │  │  ├─ Days 22-30 → Late phase         │
  │  │  └─ Days 30+ → Extended             │
  │  └─ Generate phase-appropriate advice  │
  │                                         │
  ├─[Risk Calculation] ────────────────────┘
  │  ├─ Count critical flags
  │  ├─ Count warning flags
  │  ├─ Calculate risk score
  │  │  └─ Score = (critical × 10) + (warning × 5)
  │  └─ Assign risk level:
  │     ├─ Score ≥ 20 → Critical
  │     ├─ Score ≥ 10 → High
  │     ├─ Score ≥ 5 → Medium
  │     └─ Score < 5 → Low
  │
  ├─[Recommendation Synthesis]
  │  ├─ Compile all insights
  │  ├─ Prioritize actions by urgency
  │  ├─ Generate next steps
  │  ├─ Add monitoring priorities
  │  └─ Create treatment guidance
  │
  └─[Output Generation]
     ├─ Create summary message
     ├─ Set requires-action flag
     ├─ Add timestamp
     └─ Return QuarantineCareRecommendations object
        │
        └─> END
```

---

## Statistical Methods Used

### 1. Linear Regression (Trend Detection)

```
y = mx + b

where:
  m = slope (trend direction)
  b = y-intercept

Calculation:
  m = Σ((x - x̄)(y - ȳ)) / Σ((x - x̄)²)

Interpretation:
  m > 0.01 → Rising trend
  m < -0.01 → Falling trend
  else → Stable
```

### 2. Standard Deviation (Stability)

```
σ = √(Σ(x - μ)² / N)

where:
  σ = standard deviation
  μ = mean
  N = number of data points

Interpretation:
  σ > 0.3 (for pH) → Unstable, causes stress
  σ ≤ 0.3 → Stable conditions
```

### 3. Risk Scoring (Multi-Factor)

```
Risk Score = Σ(factor_weight × factor_severity)

Example:
  Ammonia > 0.5 → +10 points (critical)
  Ammonia > 0.25 → +5 points (warning)
  Nitrite > 0.5 → +10 points (critical)
  Nitrite > 0.25 → +5 points (warning)

Risk Level Mapping:
  ≥20 → Critical (immediate action)
  10-19 → High (action within 24h)
  5-9 → Medium (monitor closely)
  <5 → Low (optimal conditions)
```

---

## File Structure

```
AquaHub.MVC/
│
├── Services/
│   ├── Interfaces/
│   │   ├── IQuarantineCareAdvisorService.cs ← Interface + Result Model
│   │   ├── IWaterChemistryPredictionService.cs
│   │   └── IYourFutureAIService.cs ← Your new service interface
│   │
│   ├── QuarantineCareAdvisorService.cs ← Implementation (550+ lines)
│   ├── WaterChemistryPredictionService.cs
│   └── YourFutureAIService.cs ← Your new service implementation
│
├── Controllers/
│   └── TankController.cs
│       ├── Constructor: Inject IQuarantineCareAdvisorService
│       └── Action: QuarantineDashboard(int id)
│           ├── Load data from database
│           ├── Call: await _aiService.AnalyzeAsync(...)
│           └── Pass results to view
│
├── Models/
│   └── ViewModels/
│       └── QuarantineDashboardViewModel.cs
│           └── Property: AIRecommendations
│
├── Views/
│   └── Tank/
│       └── QuarantineDashboard.cshtml
│           └── Display AI recommendations in card UI
│
├── Program.cs
│   └── builder.Services.AddScoped<IQuarantineCareAdvisorService, ...>()
│
└── Markdowns/
    ├── AI_SERVICES_DEVELOPER_GUIDE.md ← Full implementation guide
    ├── AI_QUICK_REFERENCE.md ← Quick cheat sheet
    ├── AI_QUARANTINE_CARE_GUIDE.md ← User guide
    └── AI_ARCHITECTURE_OVERVIEW.md ← This file
```

---

## Technology Stack

```
┌─────────────────────────────────────┐
│   Frontend Technologies             │
├─────────────────────────────────────┤
│ • Razor Views (.cshtml)             │
│ • Bootstrap 5.3.8 (UI Framework)    │
│ • Chart.js (Data Visualization)     │
│ • JavaScript (Client Interactivity) │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   Backend Technologies              │
├─────────────────────────────────────┤
│ • ASP.NET Core 8.0 MVC              │
│ • C# 12                             │
│ • Entity Framework Core             │
│ • Dependency Injection              │
│ • Async/Await Pattern               │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   AI & Analytics                    │
├─────────────────────────────────────┤
│ • Linear Regression                 │
│ • Statistical Analysis              │
│ • Pattern Recognition               │
│ • Trend Detection                   │
│ • Risk Scoring Algorithms           │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   Database                          │
├─────────────────────────────────────┤
│ • PostgreSQL (Production)           │
│ • SQLite (Development)              │
│ • Code-First Migrations             │
└─────────────────────────────────────┘
```

---

## Quick Start for New AI Feature

1. **Define** what you want to analyze
   - Example: "Coral growth patterns and recommend lighting adjustments"

2. **Identify** data sources needed
   - GrowthRecords, WaterTests, LightingSchedule

3. **Create** interface in `Services/Interfaces/`
   - Define `IYourService` and `YourRecommendations` class

4. **Implement** service in `Services/`
   - Write analysis algorithms
   - Calculate risk/scores
   - Generate recommendations

5. **Register** in `Program.cs`
   - `builder.Services.AddScoped<IYourService, YourService>()`

6. **Integrate** with controller
   - Inject service
   - Call in action method
   - Add to view model

7. **Display** in view
   - Create AI recommendations card
   - Color-code risk levels
   - Show insights and actions

8. **Test** thoroughly
   - Unit tests for algorithms
   - Integration tests with real data
   - User acceptance testing

9. **Document** your feature
   - Add to COMPLETED_FEATURES.md
   - Create user guide if needed
   - Comment complex algorithms

---

## Further Reading

- [AI Services Developer Guide](AI_SERVICES_DEVELOPER_GUIDE.md) - Complete implementation tutorial
- [AI Quick Reference](AI_QUICK_REFERENCE.md) - Cheat sheet and templates
- [Machine Learning Guide](MACHINE_LEARNING_PREDICTIONS_GUIDE.md) - ML concepts deep dive
- [Quarantine Care Guide](AI_QUARANTINE_CARE_GUIDE.md) - User documentation example

---

**Ready to build your AI feature? Start with the [Developer Guide](AI_SERVICES_DEVELOPER_GUIDE.md)!** 🚀
