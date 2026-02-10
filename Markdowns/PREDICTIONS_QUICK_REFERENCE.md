# Water Chemistry Predictions - Quick Reference

## 🚀 Quick Start

### Accessing Predictions

1. Navigate to **Predictions** in the main menu
2. Select your tank from the dropdown
3. Choose prediction timeframe (3, 7, 14, or 30 days)
4. View predictions and warnings

### URLs

- Main Dashboard: `/Prediction/Index`
- How It Works: `/Prediction/HowItWorks`
- Accuracy Report: `/Prediction/Accuracy/{tankId}`
- Parameter Detail: `/Prediction/Detail/{tankId}/{parameterName}`

---

## 📊 Understanding Your Dashboard

### Overall Forecast

- **Excellent**: All parameters stable and safe (>70% confidence)
- **Good**: Parameters stable with no warnings
- **Fair**: Some warnings but manageable (1-2 warnings)
- **Concerning**: Multiple warnings (3+ parameters at risk)

### Parameter Cards

Each card shows:

- **Current Value**: Latest test result
- **Predicted Value**: Where it will be in X days
- **Change**: Difference between current and predicted
- **Trend**: Direction (↑ Increasing, ↓ Decreasing, → Stable)
- **Confidence**: How reliable the prediction is (0-100%)
- **Rate of Change**: Daily change rate

### Color Coding

- 🟢 **Green (Success)**: Stable and safe
- 🟡 **Yellow (Warning)**: Changing but still safe
- 🔴 **Red (Danger)**: Predicted to go out of safe range

---

## 🎯 Using Predictions Effectively

### Daily Use

1. **Check dashboard weekly** to see overall trends
2. **Act on red warnings** - take corrective action before problems occur
3. **Monitor confidence scores** - low confidence means more frequent testing needed
4. **Track patterns** - understand your tank's normal behavior

### When to Test

- **After any major change**: Equipment, livestock, feeding
- **If predictions are unexpected**: Verify with actual test
- **When confidence is low**: More data improves predictions
- **Regular schedule**: Every 3-7 days for best results

### Taking Action on Warnings

#### pH Dropping

- Check alkalinity/KH
- Consider buffer additives
- Review CO2 levels (if applicable)

#### Ammonia/Nitrite Rising

- Check filter function
- Reduce feeding temporarily
- Perform water change
- Test for dead livestock

#### Nitrate Increasing

- Schedule water change
- Review feeding amounts
- Check filter media
- Consider nutrient export methods

#### Salinity Drifting

- Check for evaporation
- Verify ATO function
- Monitor temperature

---

## 🔧 Troubleshooting

### "Insufficient Data" Message

**Problem**: Not enough tests to make predictions  
**Solution**:

- Record at least 10-15 water tests
- Test at regular intervals (every 3-7 days)
- Ensure parameters have non-null values

### Low Confidence Scores (<50%)

**Problem**: Data is too erratic or inconsistent  
**Solution**:

- Test more frequently
- Ensure accurate testing technique
- Check for equipment issues causing instability
- Allow tank to stabilize after changes

### Predictions Seem Wrong

**Problem**: Predicted values don't match expectations  
**Solution**:

- Check if tank conditions recently changed
- Review historical test data for errors
- Run the Accuracy Report to see model performance
- Verify current water test is accurate

### No Predictions for a Parameter

**Problem**: Parameter not showing up  
**Solution**:

- Ensure you're testing that parameter (not null)
- Check that tank type supports that parameter
- Verify at least 10 tests have that parameter recorded

---

## 📈 Improving Prediction Accuracy

### Best Practices

1. **Test regularly** - Same day/time each week
2. **Be consistent** - Use same test kit and method
3. **Record immediately** - Don't wait days to enter results
4. **Test full panel** - More parameters = better insights
5. **Note changes** - Document when you change things

### Data Quality

- **Accurate readings**: Follow test kit instructions precisely
- **Proper storage**: Store kits correctly (avoid expiration)
- **Calibrate equipment**: For digital testers
- **Cross-check**: Verify unusual readings with second test

### Understanding Limitations

- Predictions assume **current conditions continue**
- **Not a replacement** for regular testing
- Best for **gradual changes**, not sudden events
- **More data = better predictions**

---

## 🧠 Machine Learning Basics

### What the System Does

1. Collects your historical water test data
2. Finds patterns using **Linear Regression**
3. Extends the trend line into the future
4. Calculates confidence based on data quality
5. Warns if prediction goes out of safe range

### Key Concepts

**Linear Regression**: Finds the straight line that best fits your data

- Example: If pH drops 0.1 every 3 days, predicts it will continue

**R² (Confidence Score)**: Measures how well data fits the trend line

- 1.0 = Perfect fit (100% confidence)
- 0.7 = Good fit (70% confidence)
- 0.5 = Moderate fit (50% confidence)
- 0.0 = No pattern (0% confidence)

**Rate of Change**: How fast parameter changes per day

- Negative = Decreasing
- Positive = Increasing
- Near zero = Stable

---

## 🎓 Educational Resources

### In-App

- **How It Works Page**: Detailed ML explanation
- **Accuracy Report**: See how well predictions perform
- **Parameter Details**: Deep dive into single parameter

### Documentation

- `MACHINE_LEARNING_PREDICTIONS_GUIDE.md` - Complete technical guide
- Inline code comments - Extensive explanations
- This quick reference - At-a-glance information

---

## 📋 Checklist for New Users

- [ ] Record at least 10 water tests
- [ ] Test at consistent intervals (e.g., every 3 days)
- [ ] View first predictions
- [ ] Read "How It Works" page
- [ ] Check confidence scores
- [ ] Act on any warnings
- [ ] Run accuracy report after 30 days
- [ ] Adjust testing frequency based on results

---

## 💡 Pro Tips

1. **Start with weekly testing** until patterns emerge
2. **Don't panic on low confidence** - just means need more data
3. **Use predictions to plan ahead** - order supplies before problems
4. **Track seasonality** - patterns may change with seasons
5. **Document changes** - helps explain prediction changes
6. **Compare predictions to actuals** - learn your tank's behavior

---

## ⚠️ Important Reminders

- ✅ Predictions are **estimates based on patterns**
- ✅ Always **continue regular testing**
- ✅ Use as **early warning system**, not replacement for testing
- ✅ Take action when warnings appear
- ✅ Accuracy improves with more data

---

## 🆘 Need Help?

1. Review the **How It Works** page in the app
2. Check the **Accuracy Report** to see model performance
3. Read the full guide: `MACHINE_LEARNING_PREDICTIONS_GUIDE.md`
4. Verify your testing technique is accurate
5. Ensure sufficient data (10+ tests minimum)

---

**Last Updated**: February 2026  
**Version**: 1.0  
**Feature**: Water Chemistry Predictions (Machine Learning)
