# AI-Powered Quarantine Care Advisor

## Overview

The Quarantine Care Advisor is an intelligent system that analyzes water chemistry, dosing protocols, maintenance history, and quarantine duration to provide AI-powered recommendations for caring for sick or quarantined livestock.

**Added:** February 10, 2026

---

## Features

### 1. **Real-Time Risk Assessment**

- **Critical** - Immediate action required (toxic parameters, critical conditions)
- **High** - Multiple concerns detected, increase monitoring
- **Medium** - Some issues present, monitor closely
- **Low** - All conditions optimal

### 2. **Water Chemistry Analysis**

- Analyzes trends across multiple test results
- Detects rising, falling, or stable parameter patterns
- Identifies critical thresholds for ammonia, nitrite, nitrate
- Monitors pH stability (high variance indicates stress)
- Temperature analysis with optimal ranges

**Example Insights:**

- "🔴 CRITICAL: Ammonia is 0.50 ppm. Rising trend detected - increase water changes immediately!"
- "✅ Nitrite levels are safe (0 ppm). Nitrogen cycle is established."
- "⚠️ pH is fluctuating significantly. Unstable pH causes stress - check KH/alkalinity."

### 3. **Medication & Treatment Guidance**

Intelligent dosing protocol evaluation with medication-specific advice:

#### Copper-Based Treatments (Cupramine, Copper)

- Daily copper testing requirements (0.15-0.25 ppm for saltwater)
- Carbon filtration warnings
- Fish stress monitoring (rapid breathing, hiding)
- 14-21 day minimum treatment duration
- Ammonia interaction warnings

#### Praziquantel

- Single dose or 3-dose protocols
- Safe for inverts and most fish
- 3-5 day improvement monitoring
- 7-day follow-up dose recommendations

#### Metronidazole (Metroplex)

- Internal parasite and hole-in-head disease treatment
- 3-day dosing protocol
- Appetite and behavior monitoring

#### Kanamycin (Kanaplex)

- Bacterial infection treatment
- 48-72 hour improvement window
- Biological filtration impact warnings
- 5-7 day full course
- Increased aeration recommendations

### 4. **Duration-Based Recommendations**

#### Early Quarantine (Days 1-7)

- Daily water parameter testing
- Close disease/stress observation
- Stable temperature and pH maintenance
- Small frequent water changes (10-15% daily)
- Baseline behavior documentation

#### Mid Quarantine (Days 8-21)

- Testing frequency: every 2-3 days
- Disease symptom evaluation
- Treatment effectiveness assessment
- Regular water changes (25-30% twice weekly)
- Feeding and behavior normalization

#### Late Quarantine (Days 22-30)

- Final observation period
- Reintroduction planning
- Main tank parameter matching
- Prophylactic treatment considerations
- Acclimation plan preparation

#### Extended Quarantine (30+ days)

- Ongoing treatment evaluation
- Reintroduction criteria assessment
- Medication fatigue monitoring
- Extended isolation stress evaluation

### 5. **Maintenance Recommendations**

- Water change frequency based on quarantine stage
- Filter maintenance schedules
- Aeration adjustments during treatment
- Equipment monitoring guidance

### 6. **Monitoring Priorities**

Prioritized action items:

- 🔴 **CRITICAL** - Daily testing of toxic parameters
- 🟠 **HIGH** - Daily parameter testing during first week
- 🟡 **MEDIUM** - Every 2-3 days testing, daily visual checks
- 🟢 **LOW** - Weekly full panel tests, photo documentation

### 7. **Next Steps Suggestions**

- Water change scheduling
- Testing reminders
- Parameter-specific actions
- Observation guidelines
- Documentation recommendations

### 8. **Treatment Protocol Guidance**

Customized based on quarantine purpose:

- **Disease Treatment:** Symptom reduction, improved appetite, normal behavior
- **Observation:** No symptoms after 28 days
- **Acclimation:** Eating well, exploring tank, reduced hiding
- Phase-specific guidance (Initial, Middle, Final)

---

## How It Works

### 1. Data Collection

The system analyzes:

- **Recent Water Tests** (last 14 days)
- **Dosing Records** (all treatments/medications)
- **Maintenance History** (water changes, filter cleaning)
- **Quarantine Details** (start date, purpose, status)
- **Quarantined Livestock** (species, notes)

### 2. AI Analysis

The service performs:

- **Trend Analysis** - Linear regression on parameter series
- **Statistical Analysis** - Standard deviation for pH stability
- **Pattern Recognition** - Identifies medication types and protocols
- **Risk Calculation** - Evaluates multiple factors for overall risk level
- **Context-Aware Recommendations** - Adjusts based on quarantine stage

### 3. Recommendation Generation

Generates comprehensive guidance including:

- Overall health assessment
- Water chemistry insights
- Dosing recommendations
- Maintenance actions
- Monitoring priorities
- Next steps
- Treatment-specific guidance

---

## Usage

### Accessing the Dashboard

1. Navigate to **Tanks** → Your Tank → **Details**
2. If the tank is marked as a quarantine tank, click **Quarantine Dashboard**
3. The AI recommendations appear at the top in a prominent blue card

### Understanding Risk Levels

#### ✅ **Low Risk (Green)**

- All parameters within safe ranges
- Treatment progressing normally
- Continue current care protocol

#### ⚠️ **Medium Risk (Blue)**

- One concerning parameter detected
- Monitor closely
- Take corrective action as suggested

#### ⚠️ **High Risk (Yellow)**

- Multiple concerns detected
- Increase monitoring frequency
- Address issues promptly

#### 🔴 **Critical Risk (Red)**

- Immediate action required
- Toxic parameters or critical conditions
- Emergency water change needed
- Retest within 24 hours

### Interpreting Recommendations

#### Water Chemistry Insights

- Shows actual parameter values
- Indicates trends (rising ↑, falling ↓, stable -)
- Color-coded severity (🔴 critical, ⚠️ warning, ✅ optimal)

#### Dosing Recommendations

- Medication-specific guidelines
- Treatment duration tracking
- Dosing frequency reminders
- Interaction warnings

#### Next Steps

- Prioritized action items
- Specific timelines
- Testing schedules
- Maintenance tasks

---

## Best Practices

### 1. **Test Regularly**

- First week: Daily
- Weeks 2-3: Every 2-3 days
- Week 4+: Weekly
- Always test before and after water changes

### 2. **Document Everything**

- Log all treatments/medications
- Record water parameters
- Note behavior changes
- Track feeding response

### 3. **Follow Treatment Protocols**

- Complete full treatment course
- Don't skip doses
- Monitor for improvement
- Consult veterinarian if no improvement

### 4. **Maintain Water Quality**

- Pristine water aids recovery
- More frequent changes during treatment
- Monitor ammonia/nitrite closely
- Increase aeration if needed

### 5. **Be Patient**

- Quarantine minimum: 14 days for observation, 21-30 for treatment
- Don't rush reintroduction
- Ensure complete recovery
- Match main tank parameters before transfer

---

## Technical Implementation

### Architecture

```
QuarantineCareAdvisorService (Services/QuarantineCareAdvisorService.cs)
    ├── AnalyzeQuarantineConditionsAsync() - Main analysis method
    ├── AnalyzeWaterChemistryTrends() - Parameter trend analysis
    ├── EvaluateDosingProtocol() - Medication guidance
    ├── GetDurationBasedRecommendations() - Stage-specific advice
    ├── SuggestNextSteps() - Actionable recommendations
    └── Helper methods for statistical analysis
```

### Key Classes

#### `IQuarantineCareAdvisorService`

- Interface for dependency injection
- Located: `Services/Interfaces/IQuarantineCareAdvisorService.cs`

#### `QuarantineCareRecommendations`

- Data model for AI recommendations
- Properties:
  - `OverallAssessment` - Summary message
  - `RiskLevel` - Low, Medium, High, Critical
  - `WaterChemistryInsights` - Parameter analysis
  - `DosingRecommendations` - Treatment guidance
  - `MaintenanceActions` - Care tasks
  - `MonitoringPriorities` - What to watch
  - `NextSteps` - Actionable items
  - `TreatmentGuidance` - Protocol details
  - `RequiresImmediateAction` - Urgency flag

### Integration Points

#### TankController

- Injects `IQuarantineCareAdvisorService`
- Calls `AnalyzeQuarantineConditionsAsync()` in `QuarantineDashboard()` action
- Passes recommendations to view via `QuarantineDashboardViewModel`

#### QuarantineDashboard View

- Displays AI recommendations in prominent card
- Color-codes risk levels
- Organizes insights into logical sections
- Provides expandable treatment guidance

---

## Example Scenarios

### Scenario 1: New Fish in Quarantine (Day 3)

**Analysis:**

- Duration: Early phase
- Water: Slight ammonia spike (0.10 ppm)
- Risk: Medium

**Recommendations:**

- ⚠️ Ammonia detected (0.10 ppm). Continue monitoring closely
- 📍 Early Quarantine Phase (Day 3): Test parameters daily
- 🚰 Schedule water change within 1-2 days
- 👁️ Observe behavior, appetite, physical appearance daily

### Scenario 2: Copper Treatment (Day 12)

**Analysis:**

- Duration: Mid-treatment
- Medication: Copper detected
- Water: Stable parameters

**Recommendations:**

- 🔬 Copper Treatment Detected: Test copper daily (0.15-0.25 ppm)
- 📅 Day 12 of treatment. Monitor for signs of improvement
- ✅ Water parameters are optimal
- 💧 Mid quarantine: 25-30% water changes 2-3x per week

### Scenario 3: Critical Ammonia Spike

**Analysis:**

- Ammonia: 0.60 ppm (rising)
- Nitrite: 0.25 ppm
- Risk: CRITICAL

**Recommendations:**

- 🔴 CRITICAL: Ammonia is 0.60 ppm. Rising trend - increase water changes immediately!
- 🔴 IMMEDIATE ACTION REQUIRED: Critical ammonia level
- 🚰 IMMEDIATE: Perform 25-30% water change
- 💨 INCREASE AERATION: Toxic parameters detected
- ⚠️ Test again in 24 hours to monitor trends

---

## Medication Reference

### Common Medications Recognized

- Copper/Cupramine
- Praziquantel/Prazi
- Metronidazole/Metroplex
- Kanamycin/Kanaplex
- (System will recognize variations in naming)

### Adding Custom Medications

Log dosing using the actual medication name. The AI will:

- Provide general treatment guidance
- Monitor dosing frequency
- Track treatment duration
- Suggest improvement timelines

---

## Troubleshooting

### "No AI recommendations appear"

- Ensure tank is marked as quarantine (`IsQuarantineTank = true`)
- Check that service is registered in `Program.cs`
- Verify you're on the QuarantineDashboard view, not regular Details

### "Recommendations seem generic"

- Add more water test data (AI needs trends)
- Log all treatments/medications
- Record maintenance activities
- Be specific with quarantine purpose

### "Risk level seems incorrect"

- Verify all water parameters are logged accurately
- Check that dosing records match actual treatments
- Ensure dates are correct (AI analyzes timelines)

---

## Future Enhancements

Potential improvements:

- Machine learning model training on user data
- Predictive alerts before parameters spike
- Medication interaction warnings
- Integration with external databases (disease identification)
- Photo analysis for disease symptom recognition
- Voice/chat interface for quick queries
- Mobile app push notifications for critical alerts

---

## Related Features

- [Quarantine Tank Management](QUARANTINE_TANK_GUIDE.md)
- [Water Chemistry Testing](../README.md#water-testing)
- [Dosing Records](../README.md#dosing)
- [Predictive Water Chemistry](PREDICTIVE_WATER_CHEMISTRY_README.md)

---

## Support

For questions or issues:

1. Check this guide first
2. Review [Quarantine Tank Guide](QUARANTINE_TANK_GUIDE.md)
3. Ensure all data is logged correctly
4. Verify proper quarantine setup

**Remember:** The AI recommendations are guidance, not absolute rules. Always use your judgment and consult with aquatic veterinarians for serious health issues.
