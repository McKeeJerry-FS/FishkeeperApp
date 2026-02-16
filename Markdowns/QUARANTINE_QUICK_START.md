# Quarantine Dashboard - Quick Start Guide

## Overview

The AI-powered Quarantine Dashboard helps you monitor and care for sick or newly arrived livestock with intelligent recommendations based on water chemistry, dosing protocols, and quarantine duration.

## How to Access the Quarantine Dashboard

### Step 1: Mark a Tank as Quarantine

1. Navigate to **Tanks** in the main menu
2. Either:
   - Click **"Add New Tank"** to create a new quarantine tank
   - Click **"Edit"** on an existing tank to convert it

3. In the form, scroll down to the **"Quarantine Settings"** section (yellow/warning-colored card)
4. Check the box: **"This is a Quarantine Tank"**
5. Additional fields will appear - fill them out:
   - **Quarantine Start Date**: When quarantine began
   - **Quarantine End Date**: Expected completion date (optional)
   - **Quarantine Purpose**: Select from dropdown:
     - Treatment - Treating sick fish
     - Observation - Monitoring new arrivals
     - Acclimation - Gradual adjustment
     - Prophylaxis - Preventive care
     - Recovery - Post-treatment recovery
   - **Quarantine Status**: Current state (Active/Monitoring/Completed)
   - **Treatment Protocol**: Describe your treatment plan
     - Example: "Copper treatment 0.15-0.25 ppm for 21 days, daily water changes 10%"

6. Click **"Save Tank"** or **"Update Tank"**

### Step 2: Navigate to the Dashboard

1. Go to **Tanks** → **Details** (for your quarantine tank)
2. Look for the **"Quarantine Dashboard"** button or link
3. Click to open the dashboard

### Step 3: View AI Recommendations

The dashboard displays:

- **AI Care Recommendations Card** (top section)
  - Risk Assessment with color-coded severity
  - Key Alerts about concerning conditions
  - Water Chemistry Insights with trend analysis
  - Dosing Protocol Evaluation
  - Next Steps with actionable recommendations
- **Water Test History Chart**
  - pH trends over time
  - Ammonia, Nitrite, Nitrate levels
  - Temperature monitoring
  - Salinity tracking

- **Recent Dosing Records**
  - Medications and treatments administered
  - Dosage amounts and dates
  - AI evaluation of dosing frequency

- **Quarantine Timeline**
  - Days in quarantine
  - Treatment stages (Early/Mid/Late/Extended)
  - Progress tracking

## Understanding AI Recommendations

### Risk Levels

- 🔴 **Critical** (Red badge) - Immediate action required
- 🟠 **Warning** (Orange badge) - Attention needed soon
- 🟢 **Normal** (Green badge) - Conditions stable

### Quarantine Stages

The AI adapts recommendations based on how long the tank has been in quarantine:

- **Early Stage** (0-7 days): Baseline establishment, stress reduction
- **Mid Stage** (8-14 days): Active treatment, close monitoring
- **Late Stage** (15-28 days): Recovery phase, stability focus
- **Extended Stage** (29+ days): Long-term care, reintroduction planning

### Water Chemistry Trends

The AI uses **linear regression** to detect trends:

- **Rising** ⬆️ - Parameter increasing over time
- **Falling** ⬇️ - Parameter decreasing over time
- **Stable** ➡️ - No significant change

### Dosing Evaluation

Based on last dosing date and typical medication schedules:

- ✅ **Appropriate** - Dosing frequency matches protocol
- ⚠️ **Check Schedule** - Review timing for next dose
- 🚨 **Overdue** - Dose may be needed

## Best Practices

### Data for Better Recommendations

The AI needs data to provide accurate recommendations. Make sure to log:

1. **Water Tests** (daily or every 2-3 days minimum)
   - pH, Ammonia, Nitrite, Nitrate
   - Temperature, Salinity
2. **Dosing Records** whenever you add medication
3. **Observations** in Notes field

### Quarantine Duration

Typical quarantine periods:

- **Observation**: 14-21 days
- **Treatment**: 21-28 days (copper, antibiotics)
- **Recovery**: 7-14 days post-treatment
- **Prophylaxis**: 14 days minimum

### Treatment Protocols

Common examples to enter in Treatment Protocol field:

- Copper treatment: "0.15-0.25 ppm copper for 21 days, test daily"
- Freshwater dip: "5-minute dip in dechlorinated freshwater, observe for 48 hours"
- Antibiotics: "Kanamycin 200mg/10gal every 48 hours for 7 days"
- Praziquantel: "2mg/L treatment, 5 days, then 30% water change"

## Troubleshooting

### Dashboard Not Showing?

- Verify **IsQuarantineTank** checkbox is checked
- Ensure tank is saved
- Refresh the Details page

### No AI Recommendations?

- Add water test data (need at least 2-3 tests for trends)
- Log dosing records if treating with medication
- Check that Quarantine Start Date is set

### Risk Level Seems Wrong?

The AI considers:

- Water parameter values AND trends
- Dosing frequency vs. expected schedule
- Quarantine duration stage
- Number of data points available

More data = more accurate recommendations.

## Related Documentation

- [AI Services Developer Guide](AI_SERVICES_DEVELOPER_GUIDE.md) - Learn how the AI works
- [AI Architecture Overview](AI_ARCHITECTURE_OVERVIEW.md) - System design details
- [Supply Tracking Guide](SUPPLY_TRACKING_README.md) - Track medication inventory
- [Breeding Water Monitoring](BREEDING_WATER_MONITORING.md) - Water quality best practices

## Support

If you encounter issues:

1. Check the [Known Issues](KNOWN_ISSUES.md) document
2. Verify all required fields are filled in the Tank Edit form
3. Ensure you have ASP.NET Core 8.0 and Entity Framework Core installed
4. Check browser console for JavaScript errors (F12 Developer Tools)

---

**Quick Reference Commands:**

- Create Quarantine Tank: Tanks → Add New Tank → Enable Quarantine Settings
- View Dashboard: Tanks → Details → Quarantine Dashboard
- Add Water Test: Navigate to Water Tests → Add Test for your quarantine tank
- Log Medication: Navigate to Dosing Records → Add Record for quarantine tank
