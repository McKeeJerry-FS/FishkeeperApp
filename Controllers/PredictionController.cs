using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Controllers;

/// <summary>
/// Controller for Water Chemistry Prediction features
/// </summary>
/// <remarks>
/// WHAT THIS CONTROLLER DOES:
/// 
/// This controller provides the web interface (the pages you see) for the ML prediction system.
/// It handles:
/// 1. Displaying prediction dashboards
/// 2. Generating new predictions
/// 3. Showing prediction accuracy/validation
/// 4. Explaining predictions to users
/// 
/// MVC PATTERN EXPLAINED:
/// - Model: Data structures (WaterChemistryPrediction, TankPredictionSummary)
/// - View: HTML pages that users see
/// - Controller: This file - connects Model and View, handles user requests
/// 
/// User clicks button → Controller receives request → Controller asks Service for data →
/// Service performs ML calculations → Controller sends results to View → User sees predictions
/// </remarks>
[Authorize] // Users must be logged in to see predictions
public class PredictionController : Controller
{
    private readonly IWaterChemistryPredictionService _predictionService;
    private readonly ITankService _tankService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<PredictionController> _logger;

    /// <summary>
    /// Constructor: Dependency Injection
    /// </summary>
    /// <remarks>
    /// DEPENDENCY INJECTION EXPLAINED:
    /// 
    /// Instead of creating services ourselves (like: var service = new PredictionService()),
    /// we let ASP.NET Core provide them. Benefits:
    /// - Services are shared across requests (efficient)
    /// - Easy to test (can inject fake services for testing)
    /// - Services can be configured in one place (Program.cs)
    /// 
    /// When this controller is created, ASP.NET Core automatically fills in these parameters
    /// with the registered services.
    /// </remarks>
    public PredictionController(
        IWaterChemistryPredictionService predictionService,
        ITankService tankService,
        UserManager<AppUser> userManager,
        ILogger<PredictionController> logger)
    {
        _predictionService = predictionService;
        _tankService = tankService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard showing predictions for a specific tank
    /// </summary>
    /// <remarks>
    /// This is the main page users see. It shows:
    /// - All parameter predictions
    /// - Overall tank health forecast
    /// - Warnings for parameters going out of range
    /// - Confidence scores for each prediction
    /// 
    /// ASYNC/AWAIT EXPLAINED:
    /// 
    /// "async" and "await" are used for operations that might take time (like database queries).
    /// Instead of freezing the app while waiting, it allows other work to continue.
    /// 
    /// Example:
    /// - User requests predictions
    /// - While waiting for database, app can handle other users' requests
    /// - When database responds, execution continues
    /// - Much more efficient than blocking and waiting!
    /// 
    /// Think of it like ordering food: You order (start task), do other things while waiting (async),
    /// get notified when ready (await), then continue eating (resume execution).
    /// </remarks>
    /// <param name="tankId">The tank to show predictions for</param>
    /// <param name="daysAhead">How many days ahead to predict (default 7)</param>
    [HttpGet] // This responds to GET requests (when user navigates to the page)
    public async Task<IActionResult> Index(int? tankId, int daysAhead = 7)
    {
        try
        {
            // Step 1: Get the current user's ID
            // Security Note: We ALWAYS check user ID to ensure users only see their own data
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to predictions");
                return Unauthorized();
            }

            // Step 2: Get user's tanks for the dropdown selector
            var tanks = await _tankService.GetAllTanksAsync(userId);

            if (!tanks.Any())
            {
                // User has no tanks - show message
                ViewBag.Message = "You need to create a tank before viewing predictions.";
                return View(new TankPredictionSummary { HasSufficientData = false });
            }

            // Step 3: Determine which tank to show predictions for
            int selectedTankId;
            if (tankId.HasValue)
            {
                selectedTankId = tankId.Value;
            }
            else
            {
                // If no tank specified, use the first one
                selectedTankId = tanks.First().Id;
            }

            // Step 4: Validate that the user owns this tank (security check)
            var selectedTank = tanks.FirstOrDefault(t => t.Id == selectedTankId);
            if (selectedTank == null)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access predictions for tank {TankId} they don't own",
                    userId,
                    selectedTankId);
                return Forbid(); // HTTP 403 - you don't have permission
            }

            // Step 5: Validate daysAhead parameter
            if (daysAhead < 1 || daysAhead > 30)
            {
                daysAhead = 7; // Default to 1 week
            }

            // Step 6: Generate predictions using the ML service
            _logger.LogInformation(
                "Generating predictions for tank {TankId}, user {UserId}, {DaysAhead} days ahead",
                selectedTankId,
                userId,
                daysAhead);

            var predictions = await _predictionService.GeneratePredictionsForTankAsync(
                selectedTankId,
                userId,
                daysAhead);

            // Step 7: Prepare data for the view
            // ViewBag: A way to send extra data to the view
            ViewBag.Tanks = new SelectList(tanks, "Id", "Name", selectedTankId);
            ViewBag.SelectedTankId = selectedTankId;
            ViewBag.SelectedTankName = selectedTank.Name;
            ViewBag.DaysAhead = daysAhead;

            _logger.LogInformation(
                "Successfully generated {Count} predictions for tank {TankId}",
                predictions.Predictions.Count,
                selectedTankId);

            // Step 8: Return the view with prediction data
            // The view (Index.cshtml) will display this data to the user
            return View(predictions);
        }
        catch (Exception ex)
        {
            // Error handling: Log the error and show user-friendly message
            _logger.LogError(ex, "Error generating predictions");
            ViewBag.ErrorMessage = "An error occurred while generating predictions. Please try again.";
            return View(new TankPredictionSummary { HasSufficientData = false });
        }
    }

    /// <summary>
    /// Detailed view for a single parameter prediction
    /// </summary>
    /// <remarks>
    /// This shows deep-dive information for one parameter:
    /// - Historical data chart
    /// - Trend line visualization
    /// - Detailed statistics
    /// - Recommended actions
    /// 
    /// This helps users understand WHY a prediction was made.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> Detail(int tankId, string parameterName, int daysAhead = 7)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate parameter name (prevent injection attacks)
            var validParameters = new[]
            {
                "PH", "Temperature", "Ammonia", "Nitrite", "Nitrate",
                "Salinity", "Alkalinity", "Calcium", "Magnesium", "Phosphate",
                "GH", "KH", "TDS"
            };

            if (!validParameters.Contains(parameterName))
            {
                _logger.LogWarning("Invalid parameter name: {Parameter}", parameterName);
                return BadRequest("Invalid parameter name");
            }

            // Get the tank
            var tank = await _tankService.GetTankByIdAsync(tankId, userId);
            if (tank == null)
            {
                return NotFound();
            }

            // Generate detailed prediction
            var prediction = await _predictionService.GenerateParameterPredictionAsync(
                tankId,
                parameterName,
                userId,
                daysAhead);

            if (prediction == null)
            {
                ViewBag.Message = $"Insufficient data to predict {parameterName}. Record more water tests!";
                ViewBag.TankName = tank.Name;
                ViewBag.ParameterName = parameterName;
                return View();
            }

            ViewBag.TankName = tank.Name;
            ViewBag.TankId = tankId;

            return View(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating parameter detail");
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Validation report showing prediction accuracy
    /// </summary>
    /// <remarks>
    /// MODEL VALIDATION IS CRITICAL IN ML:
    /// 
    /// This page shows how accurate our predictions have been by comparing:
    /// - Past predictions we made
    /// - Actual water test results that happened later
    /// 
    /// This helps users (and developers) know if the ML model is working well.
    /// 
    /// Example:
    /// - Jan 1: Predicted pH would be 7.8 on Jan 8
    /// - Jan 8: Actual pH was 7.9
    /// - Error: 0.1 (1.3% error)
    /// - This is good accuracy!
    /// 
    /// If errors are consistently high, we need to improve the model.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> Accuracy(int tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tank = await _tankService.GetTankByIdAsync(tankId, userId);
            if (tank == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Generating accuracy report for tank {TankId}", tankId);

            // Generate the accuracy report
            var report = await _predictionService.ValidatePredictionAccuracyAsync(tankId, userId);

            ViewBag.TankName = tank.Name;
            ViewBag.TankId = tankId;

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating accuracy report");
            ViewBag.ErrorMessage = "Error generating accuracy report.";
            return View(new PredictionAccuracyReport());
        }
    }

    /// <summary>
    /// Educational page explaining how predictions work
    /// </summary>
    /// <remarks>
    /// This is a static information page that explains:
    /// - What machine learning is
    /// - How we make predictions
    /// - What the numbers mean (confidence, R², etc.)
    /// - How to interpret predictions
    /// - Tips for improving prediction accuracy
    /// 
    /// Think of this as the "help" or "about" page for the ML features.
    /// </remarks>
    [HttpGet]
    public IActionResult HowItWorks()
    {
        // This just shows a view with educational content
        // No data processing needed - it's a static information page
        return View();
    }

    /// <summary>
    /// API endpoint to regenerate predictions (AJAX call from JavaScript)
    /// </summary>
    /// <remarks>
    /// AJAX EXPLAINED:
    /// 
    /// AJAX allows JavaScript in the browser to request data without reloading the page.
    /// 
    /// Traditional web:
    /// - User clicks button → Page reloads → New page shown
    /// 
    /// AJAX:
    /// - User clicks button → JavaScript makes background request → 
    ///   Data received → JavaScript updates part of page
    /// 
    /// This method returns JSON (JavaScript Object Notation) instead of HTML.
    /// JavaScript receives the JSON and updates the page dynamically.
    /// 
    /// Example use: "Refresh Predictions" button that updates data without reloading page.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Regenerate(int tankId, int daysAhead = 7)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "Not authorized" });
            }

            _logger.LogInformation(
                "Regenerating predictions for tank {TankId}, user {UserId}",
                tankId,
                userId);

            var predictions = await _predictionService.GeneratePredictionsForTankAsync(
                tankId,
                userId,
                daysAhead);

            // Return JSON response
            return Json(new
            {
                success = true,
                data = predictions,
                message = "Predictions updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating predictions");
            return Json(new
            {
                success = false,
                error = "Error generating predictions"
            });
        }
    }

    /// <summary>
    /// Get historical predictions for a tank (for charting)
    /// </summary>
    /// <remarks>
    /// This provides historical prediction data for visualization.
    /// Used by JavaScript charting libraries to show:
    /// - Prediction trends over time
    /// - How predictions changed as more data was collected
    /// - Past vs actual comparison charts
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> History(int tankId)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, error = "Not authorized" });
            }

            var history = await _predictionService.GetHistoricalPredictionsAsync(tankId, userId);

            return Json(new { success = true, data = history });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prediction history");
            return Json(new { success = false, error = "Error retrieving history" });
        }
    }
}
