# Test Schedule Optimizer - Developer Guide

**Feature Added:** February 17, 2026  
**Status:** ✅ Complete and Production Ready

## Overview

The Test Schedule Optimizer is an intelligent feature that provides personalized water testing recommendations based on a tank's age, type, and the user's current testing habits. It helps aquarists maintain optimal water quality by suggesting appropriate testing frequencies for each parameter.

## Key Features

### 🎯 Smart Analysis

- **Tank Maturity Detection**: Automatically categorizes tanks into 5 maturity levels
- **Type-Aware Recommendations**: Different schedules for Freshwater, Saltwater, Reef, Planted tanks
- **Behavior Analysis**: Compares user's testing habits against recommendations
- **Priority Classification**: Parameters marked as Critical, High, Medium, or Low priority

### 📊 Maturity Levels

| Level           | Age Range   | Testing Focus                                     |
| --------------- | ----------- | ------------------------------------------------- |
| **New**         | 0-6 weeks   | Daily testing of ammonia/nitrite (cycling period) |
| **Cycling**     | 6-12 weeks  | Frequent testing until cycle completes            |
| **Maturing**    | 3-6 months  | Weekly testing as stability increases             |
| **Established** | 6-12 months | Bi-weekly testing for most parameters             |
| **Mature**      | 12+ months  | Monthly testing for stable parameters             |

### 🧪 Parameter-Specific Schedules

The system provides customized schedules for:

- **Core Parameters**: Ammonia, Nitrite, Nitrate, pH, Temperature
- **Saltwater/Reef**: Salinity, Alkalinity, Calcium, Magnesium, Phosphate
- **Freshwater**: GH, KH, TDS
- **Planted**: Iron, CO2

## Architecture

### Models

**TestScheduleRecommendation** (`Models/ViewModels/TestScheduleRecommendation.cs`)

- Main view model containing all recommendation data
- Tank information and maturity analysis
- Current vs recommended testing frequencies
- Suggestions and warnings

**ParameterTestSchedule**

- Individual parameter recommendations
- Frequency, importance, and reasoning
- Current testing habits (if available)
- Visual styling properties (icons, colors)

**TankMaturityLevel** (Enum)

```csharp
public enum TankMaturityLevel
{
    New = 1,          // 0-6 weeks
    Cycling = 2,      // 6-12 weeks
    Maturing = 3,     // 3-6 months
    Established = 4,  // 6-12 months
    Mature = 5        // 12+ months
}
```

### Service Layer

**ITestScheduleOptimizerService** (`Services/TestScheduleOptimizerService.cs`)

Key Methods:

```csharp
Task<TestScheduleRecommendation> GetRecommendedScheduleAsync(int tankId, string userId)
```

- Analyzes tank age and type
- Generates maturity-based recommendations
- Calculates current testing intervals
- Provides actionable suggestions

```csharp
Task<bool> CreateRemindersFromScheduleAsync(int tankId, string userId, List<string> selectedParameters)
```

- Creates reminders for selected parameters
- Integrates with existing reminder system
- Prevents duplicate reminders

### Controller Actions

**WaterTestController**

1. **TestSchedule** (GET)
   - Landing page with tank selector
   - Redirects to details when tank selected

2. **TestScheduleDetails** (GET, /WaterTest/TestScheduleDetails/{tankId})
   - Shows complete analysis for a tank
   - Displays recommendations table
   - Current vs recommended comparison
   - Warnings and suggestions

3. **CreateScheduleReminders** (POST)
   - Accepts selected parameters
   - Creates reminders via service
   - Returns to details with success/error message

### Views

**TestSchedule.cshtml**

- Tank selection interface
- Feature overview cards
- How-it-works explanation

**TestScheduleDetails.cshtml**

- Tank information header
- Status cards (age, tests recorded, maturity, last test)
- Warnings and suggestions alerts
- Recommended schedule table with color-coded rows
- Create reminders modal
- Action buttons (Log Test, View History, Back)

## Testing Frequency Logic

### Base Recommendations by Maturity

**Critical Parameters (Ammonia, Nitrite)**

- New: Every 2 days
- Cycling: Every 3 days
- Maturing: Weekly
- Established: Bi-weekly
- Mature: Monthly

**High Priority (Nitrate, pH, Temperature, Salinity)**

- New/Cycling: Weekly
- Maturing+: Bi-weekly

**Reef-Specific (Alkalinity, Calcium)**

- All levels: Weekly

**Medium Priority (Magnesium, Iron, CO2)**

- Weekly to Bi-weekly depending on tank type

**Low Priority (GH, KH, TDS)**

- Bi-weekly for most maturity levels

### Behavior Analysis

The system identifies:

- **Undertesting**: Current interval > 1.5x recommended
- **Overtesting**: Current interval < 0.5x recommended (for non-critical params)
- **Never Tested**: Required parameters with no test history
- **Optimal**: Testing aligns with recommendations

## User Workflow

1. **Access**: Navigate to Maintenance → Test Schedule Optimizer
2. **Select Tank**: Choose tank from dropdown
3. **Review**: See maturity level, recommendations, and current habits
4. **Analyze**: Read warnings and suggestions
5. **Action**:
   - Create reminders for recommended parameters
   - Log a water test immediately
   - View historical test data

## Integration Points

### Reminder System

- Automatically creates reminders with correct frequency
- Uses existing `ReminderType.WaterTest`
- Prevents duplicate reminders for same parameter/tank
- Maps days to `ReminderFrequency` enum

### Water Test Service

- Reads historical test data
- Calculates average testing intervals
- Determines last test date

### Tank Service

- Retrieves tank details
- Validates user ownership
- Provides tank type for schedule generation

## Database Queries

### Main Analysis Query

```csharp
var tank = await _context.Tanks
    .Include(t => t.WaterTests)
    .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);
```

### Reminder Creation Query

```csharp
var existingReminder = await _context.Reminders
    .FirstOrDefaultAsync(r => r.UserId == userId &&
                            r.TankId == tankId &&
                            r.Title.Contains(param.ParameterDisplayName) &&
                            r.IsActive);
```

## UI/UX Features

### Visual Indicators

**Status Classes**

- `table-success`: Testing at or better than recommended
- `table-warning`: Testing less frequently than recommended
- `table-danger`: Required parameter never tested

**Icons**

- Each parameter has a unique Bootstrap icon
- Color-coded by importance (danger, warning, info, success)

**Badges**

- Importance: Critical, High, Medium, Low
- Frequency: Daily, Weekly, Bi-weekly, Monthly
- Required vs Optional parameters

### Responsive Design

- Mobile-friendly card layout
- Collapsible sections for smaller screens
- Touch-friendly modal interactions

## Testing Recommendations

### Manual Testing Checklist

- [ ] Create a new tank (< 6 weeks old) and verify "New" maturity
- [ ] Create a tank with various ages and check maturity classification
- [ ] Test with different tank types (Freshwater, Saltwater, Reef, Planted)
- [ ] Verify parameters shown match tank type
- [ ] Add water tests and check current frequency calculations
- [ ] Test reminder creation functionality
- [ ] Verify duplicate reminder prevention
- [ ] Check warnings for untested required parameters
- [ ] Verify suggestions appear correctly
- [ ] Test with tanks that have no water tests
- [ ] Check modal form submission
- [ ] Verify navigation breadcrumbs
- [ ] Test responsive layout on mobile

### Edge Cases

- Tank with no water tests (should show warnings)
- Very old tank (> 5 years)
- Tank with sparse testing history
- All parameters tested optimally (should show success message)
- Attempting to create reminders for already-reminded parameters

## Performance Considerations

- Recommendations generated on-demand (not cached)
- Efficient single query loads tank + all water tests
- In-memory calculation of intervals
- No complex joins or aggregations in database

## Future Enhancements

Potential improvements:

1. **Seasonal Adjustments**: Modify frequency based on time of year
2. **Parameter Correlation**: Suggest testing related parameters together
3. **Historical Compliance**: Track how well user follows recommendations
4. **Cost Optimization**: Balance testing needs with test kit costs
5. **Community Benchmarking**: Compare to similar tanks
6. **AI-Powered Learning**: Adapt recommendations based on parameter stability
7. **Test Kit Integration**: Track test kit expiration and usage
8. **Email Summaries**: Weekly reports on testing compliance

## Configuration

No configuration required. The system uses hardcoded rules based on best practices from:

- Marine aquarium industry standards
- Freshwater fishkeeping guidelines
- Planted tank maintenance protocols
- Reef aquarium chemistry requirements

## Related Files

### Models

- `/Models/ViewModels/TestScheduleRecommendation.cs`
- `/Models/ViewModels/TestScheduleOptimizerViewModel.cs`
- `/Models/Tank.cs`
- `/Models/WaterTest.cs`
- `/Models/Reminder.cs`

### Services

- `/Services/TestScheduleOptimizerService.cs`
- `/Services/Interfaces/ITestScheduleOptimizerService.cs`

### Controllers

- `/Controllers/WaterTestController.cs`

### Views

- `/Views/WaterTest/TestSchedule.cshtml`
- `/Views/WaterTest/TestScheduleDetails.cshtml`

### Configuration

- `/Program.cs` (Service registration)

### Navigation

- `/Views/Shared/_Layout.cshtml`

## API Endpoints

| Method | Route                                     | Description                     |
| ------ | ----------------------------------------- | ------------------------------- |
| GET    | `/WaterTest/TestSchedule`                 | Landing page with tank selector |
| GET    | `/WaterTest/TestSchedule?tankId={id}`     | Redirects to details            |
| GET    | `/WaterTest/TestScheduleDetails/{tankId}` | Shows recommendations           |
| POST   | `/WaterTest/CreateScheduleReminders`      | Creates reminders from schedule |

## Security

- All routes require authentication (`[Authorize]`)
- Tank ownership verified before analysis
- User ID validated from claims
- Anti-forgery tokens on POST requests
- No direct database access from views

## Logging

Service logs:

- Errors during recommendation generation
- Errors during reminder creation
- Tank ID in all error messages for traceability

## Dependencies

- **EntityFramework Core**: Database access
- **ASP.NET Core Identity**: User authentication
- **Bootstrap 5**: UI components
- **Bootstrap Icons**: Visual indicators
- Existing services: `ITankService`, `IWaterTestService`

## Support Resources

- **COMPLETED_FEATURES.md**: Feature status
- **FEATURE_IDEAS.md**: Original feature request
- **Water Testing Documentation**: Parameter guidelines
- **Reminder System Documentation**: Integration details

---

**Maintainer Notes**: This feature requires no database migrations as it uses existing tables. The service is stateless and can be scaled horizontally. All recommendations are calculated in real-time based on current data.
