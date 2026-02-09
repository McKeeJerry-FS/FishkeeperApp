# Breeding Water Condition Monitoring

## Overview

The Breeding Water Condition Monitoring feature provides real-time analysis and recommendations for maintaining optimal water parameters for breeding success. This intelligent system compares current water test results against ideal breeding conditions and provides actionable recommendations.

## Features

### 1. Ideal Water Parameter Configuration

Set specific target ranges for each breeding pair:

- **Temperature Range**: Min/Max in °F (32-110°F)
- **pH Range**: Min/Max (0-14)
- **GH (General Hardness)**: Min/Max in dGH
- **KH (Carbonate Hardness)**: Min/Max in dKH
- **Ammonia Maximum**: 0-10 ppm (should ideally be 0)
- **Nitrite Maximum**: 0-10 ppm (should ideally be 0)
- **Nitrate Maximum**: 0-200 ppm

### 2. Automated Water Quality Analysis

The system automatically:

- Retrieves the latest water test for the breeding tank
- Compares each parameter against ideal ranges
- Categorizes each parameter as:
  - **Optimal** (Green): Within ideal range
  - **Warning** (Yellow): Slightly out of range or near edge
  - **Critical** (Red): Significantly out of range
  - **Unknown** (Gray): Parameter not tested

### 3. Visual Status Indicators

- **Overall Status Card**: Quick view of conditions (Optimal/Needs Adjustment/Critical)
- **Parameter Cards**: Individual cards for each water parameter with:
  - Current value
  - Ideal range
  - Visual indicator (green/yellow/red badge)
  - Visual range slider showing current position
  - Specific recommendations

### 4. Intelligent Recommendations

Context-aware recommendations including:

- **Temperature Adjustments**: Gradual heating/cooling instructions
- **pH Adjustments**: Buffer addition or pH lowering product suggestions
- **Hardness Adjustments**: GH booster or RO water usage
- **Toxic Parameter Alerts**: Immediate action for ammonia/nitrite
- **Water Change Recommendations**: Percentage and frequency suggestions
- **Test Age Alerts**: Warnings when tests are more than 3 days old

### 5. Quick Actions

Direct links to:

- Test water parameters
- View water test history and trends
- Edit breeding pair ideal parameters

## Usage

### Setting Up Ideal Parameters

1. Navigate to **Care → Breeding**
2. Create or edit a breeding pair
3. Scroll to the "Ideal Water Parameters for Breeding" section
4. Enter the optimal ranges for your species:
   ```
   Example for Angelfish:
   - Temperature: 78-82°F
   - pH: 6.5-7.0
   - GH: 3-8 dGH
   - KH: 1-5 dKH
   - Max Ammonia: 0 ppm
   - Max Nitrite: 0 ppm
   - Max Nitrate: 20 ppm
   ```

### Monitoring Water Conditions

1. From the breeding pair details page, click **"Water Conditions"**
2. View the comprehensive analysis showing:
   - Overall breeding readiness status
   - Individual parameter analysis
   - Specific recommendations for improvements
3. The page automatically refreshes every 5 minutes if left open

### Understanding Status Colors

- **Green (Optimal)**: All parameters are within ideal ranges
- **Yellow (Warning)**: Some parameters need attention
- **Red (Critical)**: Immediate action required before breeding

## Recommendation System

### Temperature Recommendations

- **Too Low**: Increase heater setting gradually (1-2°F per day)
- **Too High**: Reduce heater or add cooling
- **Optimal**: Maintain stable temperature

### pH Recommendations

- **Too Low**: Add crushed coral, limestone, or baking soda (gradually!)
- **Too High**: Use driftwood, peat, or pH down products
- **Optimal**: Monitor and maintain stability

### Hardness Recommendations

- **GH Too Low**: Add GH booster, crushed coral, or wonder shells
- **GH Too High**: Use RO water or distilled water in changes
- **KH Too Low**: Add baking soda or KH buffer for pH stability
- **KH Too High**: Use RO water to dilute

### Toxic Parameter Handling

- **Ammonia Detected**: Water change and check filter
- **Ammonia Critical**: 50% water change immediately, do not breed
- **Nitrite Detected**: Check biological filtration, water change
- **Nitrite Critical**: Tank not cycled properly, do not breed
- **Nitrate Elevated**: 25-30% water change before breeding
- **Nitrate Critical**: Larger water changes needed

## Best Practices

### Before Breeding

1. Test water parameters
2. Ensure all parameters are in "Optimal" status
3. Address any "Warning" or "Critical" issues
4. Retest to confirm improvements
5. Wait 24-48 hours after adjustments before breeding

### During Breeding

1. Monitor parameters daily
2. Maintain stable conditions (avoid sudden changes)
3. Increase feeding frequency with high-quality foods
4. Keep detailed notes on behavior and water conditions

### After Spawning

1. Continue daily monitoring
2. Maintain pristine water quality (0 ammonia, 0 nitrite)
3. Keep parameters within ideal ranges for fry development
4. Document results in breeding attempts

## Integration with Other Features

### Water Tests

- Automatically uses latest test from the breeding tank
- Links directly to water test history
- Suggests new tests if data is older than 3 days

### Breeding Attempts

- Use water condition data to determine optimal breeding timing
- Track correlation between water quality and breeding success
- Document water parameters during successful attempts

### Alerts (Future)

- Planned integration with parameter alert system
- Email notifications when conditions drift from optimal
- Reminders to test water before breeding attempts

## Technical Details

### Analysis Service

- **BreedingWaterAnalysisService**: Core analysis engine
- **BreedingWaterConditionViewModel**: Data structure for UI
- **WaterParameterStatus**: Individual parameter tracking

### Algorithm

1. Check if ideal parameters are configured
2. Retrieve latest water test for breeding tank
3. For each parameter:
   - Compare current value to ideal range
   - Calculate deviation percentage
   - Determine status (Optimal/Warning/Critical)
   - Generate context-specific recommendation
4. Generate overall readiness assessment
5. Compile prioritized recommendation list

### Status Determination

- **Optimal**: Within ideal range, not near edges
- **Warning**: Within 10% of range limits OR slightly outside range
- **Critical**: More than 10% outside range limits

## Troubleshooting

### "No Water Test Data Available"

- Click "Test Water Now" to create a new water test
- Ensure you're testing the correct tank

### "Set Ideal Water Parameters"

- Click "Edit Breeding Pair" to configure ideal ranges
- Research your species' specific breeding requirements

### Parameters Show "Unknown"

- Test all relevant parameters for your setup
- Some parameters (like GH/KH) may not be tested in saltwater setups

## Species-Specific Examples

### Freshwater Examples

#### Angelfish (Pterophyllum scalare)

```
Temperature: 78-82°F
pH: 6.5-7.0
GH: 3-8 dGH
KH: 1-5 dKH
Max Ammonia: 0 ppm
Max Nitrite: 0 ppm
Max Nitrate: 20 ppm
```

#### Discus (Symphysodon)

```
Temperature: 82-86°F
pH: 6.0-7.0
GH: 1-4 dGH
KH: 1-3 dKH
Max Ammonia: 0 ppm
Max Nitrite: 0 ppm
Max Nitrate: 10 ppm
```

#### Guppies (Poecilia reticulata)

```
Temperature: 74-82°F
pH: 7.0-8.0
GH: 8-12 dGH
KH: 3-8 dKH
Max Ammonia: 0 ppm
Max Nitrite: 0 ppm
Max Nitrate: 30 ppm
```

### Saltwater Examples

#### Clownfish (Amphiprion ocellaris)

```
Temperature: 75-82°F
pH: 8.1-8.4
Max Ammonia: 0 ppm
Max Nitrite: 0 ppm
Max Nitrate: 5 ppm
```

## Future Enhancements

### Planned Features

- **Historical Trending**: Track parameter stability over time
- **Breeding Success Correlation**: Analyze which parameters most affect success
- **Automatic Alerts**: Push notifications when parameters drift
- **Species Presets**: Pre-configured ideal ranges for common species
- **AI Recommendations**: Machine learning-based breeding timing suggestions
- **Export Reports**: PDF reports of water conditions for record-keeping

### Integration Roadmap

- Link to equipment automation (adjust heaters/dosers)
- Integration with smart testing devices
- Mobile app notifications
- Voice assistant integration for quick status checks

## Support

For issues or questions about water condition monitoring:

1. Check this documentation
2. Review breeding pair ideal parameters
3. Verify water test is recent (less than 3 days old)
4. Consult species-specific breeding guides

## Related Documentation

- [Breeding System Overview](BREEDING_README.md) (if exists)
- [Water Test Documentation](WATER_TEST_README.md) (if exists)
- [Alert System Guide](ALERTS_README.md) (if exists)
