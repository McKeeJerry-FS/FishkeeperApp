# Quarantine Tank Management

## Overview

The Quarantine Tank Management feature provides comprehensive tracking and monitoring for livestock quarantine periods. This critical feature helps ensure the health of your main display tanks by properly quarantining new arrivals, treating sick livestock, or acclimating sensitive species before introduction.

## Key Features

### 1. **Quarantine Tank Setup**

Any tank in AquaHub can be designated as a quarantine tank with dedicated tracking:

- **Quarantine Type Selection** - Mark any tank type as a quarantine tank
- **Start & End Dates** - Track the quarantine period timeline
- **Quarantine Purpose** - Document the reason (Treatment, Observation, Acclimation, etc.)
- **Status Tracking** - Monitor progression (Active, Monitoring, Completed)
- **Treatment Protocol** - Record detailed treatment plans and procedures

### 2. **Dedicated Quarantine Dashboard**

Access a specialized dashboard that consolidates all critical quarantine information:

#### **Progress Tracking**

- Visual progress bar showing days in quarantine
- Days remaining until target completion
- Overdue notifications for extended quarantines
- Clear status indicators (Active/Completed/Overdue)

#### **Water Chemistry Monitoring**

- Real-time water parameter display
- 14-day trend charts for critical parameters:
  - Ammonia (ppm)
  - Nitrite (ppm)
  - Nitrate (ppm)
  - Temperature (°F)
  - pH
  - Salinity (for saltwater)
- Automatic alerts for dangerous levels
- Visual indicators (green/yellow/red) for parameter status
- Test history with timestamps

#### **Dosing & Treatment Protocol**

- Active treatments display
- Complete dosing history
- Treatment schedule tracking
- Chemical/medication tracking
- Amount and frequency logs
- Quick access to add new doses

#### **Feeding Schedule**

- Scheduled feeding times
- Recent feeding logs
- Food type tracking
- Portion size documentation
- Multiple daily feeding support

#### **Livestock Tracking**

- List of all quarantined inhabitants
- Individual livestock details
- Addition dates
- Quick links to full livestock profiles
- Health status monitoring

#### **Maintenance Logs**

- Recent maintenance activities
- Water change tracking with day counter
- Alerts when water changes are overdue (>7 days)
- Equipment cleaning logs
- Filter maintenance records

### 3. **Smart Alerts & Recommendations**

The quarantine dashboard provides intelligent monitoring:

- **Critical Water Parameters** - Immediate alerts for dangerous ammonia/nitrite levels
- **Water Test Reminders** - Notifications when testing hasn't occurred in 3+ days
- **Water Change Alerts** - Warnings when water changes are overdue
- **Treatment Reminders** - Track dosing schedules
- **Health Concerns** - Flag potential health issues based on parameters

### 4. **Quick Action Cards**

One-click access to common quarantine tasks:

- Test Water Chemistry
- Log Maintenance/Water Change
- Add Dosing/Treatment
- Record Feeding
- Update Livestock Status

### 5. **Data Integration**

The quarantine dashboard seamlessly integrates with existing AquaHub features:

- Water Test records
- Dosing Records
- Feeding Schedules & Records
- Maintenance Logs
- Livestock profiles
- Equipment tracking

## Using Quarantine Tanks

### Creating a Quarantine Tank

1. **Create New Tank**
   - Go to Tanks → Create New Tank
   - Select "Quarantine" as the tank type (or any type and mark as quarantine)
   - Enter basic tank information (name, volume, start date)

2. **Configure Quarantine Details**
   - Mark "Is Quarantine Tank" checkbox
   - Set Quarantine Start Date
   - Set Expected Quarantine End Date (typically 28-30 days for disease prevention)
   - Specify Quarantine Purpose (Treatment, Observation, Acclimation)
   - Set Quarantine Status (Active, Monitoring, Completed)
   - Document Treatment Protocol if applicable

3. **Access Quarantine Dashboard**
   - From Tank list or Details page
   - Click "Quarantine Dashboard" button
   - View comprehensive monitoring interface

### Managing a Quarantine Period

#### Daily Monitoring

1. **Check Water Parameters** - Test ammonia, nitrite daily during first week
2. **Log Test Results** - Add water tests to track trends
3. **Record Feedings** - Document feeding schedule and livestock response
4. **Add Treatments** - Log any medications or treatments administered
5. **Observe Livestock** - Update health status and behavior notes

#### Weekly Maintenance

1. **Water Changes** - Perform and log water changes (typically 25-50%)
2. **Equipment Cleaning** - Clean filters, check equipment function
3. **Parameter Review** - Review 7-day trends for improvements
4. **Progress Assessment** - Evaluate if quarantine can conclude or needs extension

### Best Practices

**Recommended Quarantine Periods:**

- **New Fish** - 28-30 days minimum
- **Disease Treatment** - Until 7 days after symptoms resolve
- **Acclimation** - 14-21 days for sensitive species
- **Observation** - 14 days minimum

**Water Testing Frequency:**

- **Days 1-7**: Daily testing for ammonia/nitrite
- **Days 8-14**: Every other day
- **Days 15+**: Every 2-3 days or as needed

**Water Change Schedule:**

- **First Week**: 25% daily or 50% every other day
- **Week 2-4**: 25-30% twice weekly
- **As Needed**: Additional changes if parameters spike

## Technical Implementation

### Database Schema

**Tank Model Extensions:**

```csharp
public bool IsQuarantineTank { get; set; }
public DateTime? QuarantineStartDate { get; set; }
public DateTime? QuarantineEndDate { get; set; }
public string QuarantinePurpose { get; set; }
public string QuarantineStatus { get; set; }
public string TreatmentProtocol { get; set; }
```

**AquariumType Enum Addition:**

```csharp
Quarantine = 11
```

### Controller Actions

**QuarantineDashboard Action:**

- Calculates quarantine progress and timeline
- Aggregates water test data (14-day window)
- Evaluates parameter safety levels
- Retrieves dosing and feeding records
- Compiles maintenance history
- Generates alerts and recommendations

### View Model

**QuarantineDashboardViewModel** includes:

- Tank information and quarantine settings
- Progress calculation (days, percentage, overdue status)
- Water chemistry data with trend charts
- Active treatments and dosing history
- Feeding schedules and records
- Maintenance logs and water change tracking
- Smart alerts and action items

### Features Integration

The quarantine system leverages existing AquaHub infrastructure:

- **Water Tests** - Full compatibility with water testing system
- **Dosing Records** - Treatment tracking via dosing system
- **Feeding System** - Schedule and record integration
- **Maintenance Logs** - Water change and cleaning tracking
- **Livestock Management** - Direct linking to quarantined animals

## Use Cases

### 1. **New Livestock Quarantine**

"I just purchased new clownfish and need to quarantine them for 30 days before adding to my reef tank."

- Create quarantine tank or use existing
- Set 30-day quarantine period
- Add new clownfish to quarantine tank
- Monitor daily via dashboard
- Test water parameters regularly
- Feed and observe behavior
- Complete quarantine when healthy and period ends

### 2. **Disease Treatment**

"My tang has ich and needs copper treatment in isolation."

- Move tang to quarantine/hospital tank
- Set quarantine purpose as "Treatment"
- Document copper treatment protocol
- Log daily copper dosing
- Monitor water parameters closely
- Track symptoms improvement
- Continue until 7 days after symptoms clear

### 3. **Sensitive Species Acclimation**

"My mandarin dragonet needs slow acclimation before reef introduction."

- Set up species-specific quarantine
- 21-day acclimation period
- Monitor feeding success
- Adjust parameters gradually
- Observe behavior and health
- Introduce to main tank when stable

### 4. **Pre-emptive Health Screening**

"Standard 28-day observation for all new arrivals."

- Quarantine all new livestock
- Observe for latent diseases
- Allow stress recovery
- Monitor feeding response
- Verify health before main tank addition

## Benefits

### For Hobbyists

- **Disease Prevention** - Catch issues before they reach display tanks
- **Better Outcomes** - Focused care during critical periods
- **Peace of Mind** - Systematic tracking of quarantine progress
- **Data-Driven Decisions** - Clear metrics for when to complete quarantine

### For Advanced Aquarists

- **Treatment Documentation** - Complete records of protocols and results
- **Multi-Tank Management** - Dedicated tools for hospital/quarantine setups
- **Pattern Recognition** - Historical data for optimizing procedures
- **Professional Standards** - Track quarantine like commercial facilities

## Future Enhancements

Potential additions to the quarantine system:

- **Treatment Templates** - Pre-configured protocols for common diseases
- **Photo Timeline** - Visual progress documentation
- **Automatic Notifications** - Email/SMS for critical alerts
- **Quarantine Checklist** - Daily task management
- **Success Rate Tracking** - Analytics on quarantine outcomes
- **Integration with Disease Database** - Symptom matching and treatment suggestions

---

## Quick Reference

**Accessing Quarantine Dashboard:**

1. Navigate to Tanks
2. Select a quarantine tank
3. Click "Quarantine Dashboard" button

**Critical Parameters to Monitor:**

- ✅ Ammonia: 0 ppm (critical)
- ✅ Nitrite: 0 ppm (critical)
- ✅ Nitrate: < 40 ppm
- ✅ Temperature: 74-80°F
- ✅ pH: 7.8-8.4 (saltwater) or 6.5-7.5 (freshwater)

**Recommended Actions:**

- Daily water tests (first week)
- Water changes every 2-3 days minimum
- Document all treatments
- Record feeding response
- Monitor behavior changes
- Update health status regularly

---

_Last Updated: February 10, 2026_
