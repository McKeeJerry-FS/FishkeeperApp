# Planted Tank Water Parameters

## Overview

The application now includes specialized water parameter tracking for planted aquariums, with specific optimal ranges tailored for plant health and growth.

## New Parameters

### 1. Iron (Fe)

- **Range**: 0.1 - 0.5 ppm
- **Purpose**: Essential micronutrient for plant growth and chlorophyll production
- **Notes**: Deficiency causes chlorosis (yellowing) in new growth

### 2. CO₂ (Carbon Dioxide)

- **Range**: 20 - 30 ppm
- **Purpose**: Primary carbon source for photosynthesis
- **Notes**: Higher levels promote faster growth but require careful monitoring to avoid fish stress

### 3. Phosphate (PO₄)

- **Standard tanks**: ≤ 1.0 ppm (general freshwater)
- **Planted tanks**: 0.5 - 2.0 ppm
- **Purpose**: Essential macronutrient for energy transfer and growth
- **Notes**: Different ranges for planted vs. non-planted tanks

### 4. Total Dissolved Solids (TDS)

- **Planted tank range**: 150 - 250 ppm
- **Purpose**: Overall measure of dissolved minerals
- **Notes**: Already existed but now has specific planted tank ranges

## Updated Parameter Ranges for Planted Tanks

### General Hardness (GH)

- **Planted tank range**: 50 - 100 ppm
- **Standard range**: 70 - 215 ppm
- **Notes**: Planted tanks prefer softer water

### Carbonate Hardness (KH)

- **Planted tank range**: 40 - 80 ppm
- **Standard range**: 54 - 143 ppm
- **Notes**: Lower KH allows for better CO₂ utilization

## Features Implemented

### 1. Database Model

- Added `Iron` and `CO₂` as nullable double properties to `WaterTest` model
- Migration: `AddPlantedTankWaterParameters`
- Fields are optional (nullable) since they only apply to planted tanks

### 2. Water Test Forms

#### Create View

- New "Planted Tank Parameters" section appears only for planted tanks
- Input fields with ideal range hints:
  - Iron: "Ideal: 0.1 - 0.5 ppm"
  - CO₂: "Ideal: 20 - 30 ppm"
  - Phosphate: "Ideal: 0.5 - 2.0 ppm" (for planted)
- JavaScript automatically shows/hides section based on selected tank type

#### Edit View

- Same planted parameters section
- Auto-displays if tank type is Planted
- Maintains values when editing existing tests

### 3. Details View with Gauges

- Iron and CO₂ gauges display for planted tanks
- Color-coded status indicators:
  - Green: Within optimal range
  - Yellow/Orange: Approaching limits
  - Red: Out of range
- Dynamic range adjustment:
  - GH, KH, and Phosphate ranges automatically adjust for planted tanks
  - Ensures accurate status calculations

### 4. Service Layer Updates

- `WaterTestService.cs` includes planted-specific parameter trends
- Separate analysis logic for:
  - Iron (0.1 - 0.5 ppm)
  - CO₂ (20 - 30 ppm)
  - Phosphate (0.5 - 2.0 ppm for planted)
  - GH (50 - 100 ppm for planted)
  - KH (40 - 80 ppm for planted)

## Usage Guide

### Recording Water Tests for Planted Tanks

1. Navigate to Water Tests → Log New Test
2. Select your planted tank from the dropdown
3. The "Planted Tank Parameters" section will automatically appear
4. Enter values for:
   - Standard parameters (pH, Ammonia, Nitrite, Nitrate, Temperature)
   - Planted-specific parameters (Iron, CO₂, Phosphate)
5. Save the test

### Viewing Test Results

**Details View:**

- Navigate to Water Tests → Details
- Gauges show color-coded status for each parameter
- Overall water quality percentage calculated
- Planted-specific parameters only show for planted tanks

**Dashboard View:**

- Tank Dashboard displays latest water test parameters
- Gauges show at-a-glance health status
- Critical parameters highlighted if out of range

**Trends View:**

- Select your planted tank from dropdown
- Charts display parameter history over time
- Planted-specific parameters included in analysis

## Technical Implementation

### Conditional Display Logic

**JavaScript (Create/Edit forms):**

```javascript
const parameterVisibility = {
  Planted: {
    freshwater: true,
    saltwater: false,
    phosphate: false,
    planted: true, // Shows Iron, CO₂, planted-range Phosphate
  },
};
```

**Razor (Details view):**

```csharp
var isPlanted = Model.Tank.Type == AquaHub.MVC.Models.Enums.AquariumType.Planted;
if (isPlanted)
{
    // Add Iron and CO₂ gauges
    // Update GH, KH, Phosphate ranges
}
```

### Range Validation

The system uses the `ParameterGauge` helper class to:

1. Check if values exist (nullable handling)
2. Compare against min/max ideal ranges
3. Calculate percentage within range
4. Assign color codes for visual feedback

## Best Practices

### For Planted Tank Maintenance

1. **Test Regularly**: Monitor Iron and CO₂ at least weekly
2. **Watch for Deficiencies**:
   - Low Iron → Yellow/pale new leaves
   - Low CO₂ → Slow growth, algae issues
3. **Balance Nutrients**: Maintain proper NPK (Nitrate, Phosphate, Potassium) ratios
4. **Adjust Dosing**: Use test results to fine-tune fertilizer and CO₂ injection

### Parameter Relationships

- **High CO₂** + **Low KH** = More effective but requires careful monitoring
- **Iron** should be supplemented regularly (chelated iron recommended)
- **Phosphate** levels depend on plant density and lighting intensity

## Future Enhancements

Potential additions:

- Potassium (K) tracking
- Nitrate (NO₃) specific ranges for planted tanks
- Fertilizer dosing calculator based on test results
- Plant health correlation analysis
- Recommended dosing schedules

## Migration Details

**Migration Name**: `AddPlantedTankWaterParameters`
**Date**: February 2026
**Changes**:

- Added `Iron` column (nullable double)
- Added `CO2` column (nullable double)

**Apply Migration**:

```bash
dotnet ef database update
```

**Rollback**:

```bash
dotnet ef database update PreviousMigrationName
dotnet ef migrations remove
```
