namespace AquaHub.MVC.Utilities;

/// <summary>
/// Centralized logging constants for consistent structured logging across the application
/// These event IDs help categorize and filter logs for OpenTelemetry integration
/// </summary>
public static class LoggingConstants
{
    // Event ID ranges by category
    public static class EventIds
    {
        // General Application Events (1000-1099)
        public const int ApplicationStartup = 1000;
        public const int ApplicationShutdown = 1001;
        public const int ConfigurationLoaded = 1002;

        // HTTP Request Events (1100-1199)
        public const int RequestStarted = 1100;
        public const int RequestCompleted = 1101;
        public const int RequestFailed = 1102;

        // Database Events (2000-2099)
        public const int DatabaseQueryStarted = 2000;
        public const int DatabaseQueryCompleted = 2001;
        public const int DatabaseQueryFailed = 2002;
        public const int DatabaseSaveStarted = 2010;
        public const int DatabaseSaveCompleted = 2011;
        public const int DatabaseSaveFailed = 2012;
        public const int DatabaseConnectionOpened = 2020;
        public const int DatabaseConnectionClosed = 2021;

        // Service Layer Events (3000-3999)
        // Tank Service (3000-3099)
        public const int TankServiceOperationStarted = 3000;
        public const int TankServiceOperationCompleted = 3001;
        public const int TankServiceOperationFailed = 3002;
        public const int TankCreated = 3010;
        public const int TankUpdated = 3011;
        public const int TankDeleted = 3012;

        // Livestock Service (3100-3199)
        public const int LivestockServiceOperationStarted = 3100;
        public const int LivestockServiceOperationCompleted = 3101;
        public const int LivestockServiceOperationFailed = 3102;
        public const int LivestockAdded = 3110;
        public const int LivestockUpdated = 3111;
        public const int LivestockRemoved = 3112;

        // Water Test Service (3200-3299)
        public const int WaterTestServiceOperationStarted = 3200;
        public const int WaterTestServiceOperationCompleted = 3201;
        public const int WaterTestServiceOperationFailed = 3202;
        public const int WaterTestRecorded = 3210;
        public const int WaterTestAnalyzed = 3211;

        // Maintenance Service (3300-3399)
        public const int MaintenanceServiceOperationStarted = 3300;
        public const int MaintenanceServiceOperationCompleted = 3301;
        public const int MaintenanceServiceOperationFailed = 3302;
        public const int MaintenanceLogCreated = 3310;

        // Equipment Service (3400-3499)
        public const int EquipmentServiceOperationStarted = 3400;
        public const int EquipmentServiceOperationCompleted = 3401;
        public const int EquipmentServiceOperationFailed = 3402;

        // Supply Service (3500-3599)
        public const int SupplyServiceOperationStarted = 3500;
        public const int SupplyServiceOperationCompleted = 3501;
        public const int SupplyServiceOperationFailed = 3502;
        public const int SupplyLevelUpdated = 3510;
        public const int SupplyLowAlert = 3511;

        // Feeding Service (3600-3699)
        public const int FeedingServiceOperationStarted = 3600;
        public const int FeedingServiceOperationCompleted = 3601;
        public const int FeedingServiceOperationFailed = 3602;

        // Notification & Reminder Services (3700-3799)
        public const int NotificationSent = 3700;
        public const int NotificationFailed = 3701;
        public const int ReminderCreated = 3710;
        public const int ReminderTriggered = 3711;

        // Email Service (3800-3899)
        public const int EmailSendStarted = 3800;
        public const int EmailSendCompleted = 3801;
        public const int EmailSendFailed = 3802;

        // Prediction & AI Services (3900-3999)
        public const int PredictionRequested = 3900;
        public const int PredictionCompleted = 3901;
        public const int PredictionFailed = 3902;

        // Security & Authentication Events (4000-4099)
        public const int UserAuthenticated = 4000;
        public const int UserAuthenticationFailed = 4001;
        public const int UnauthorizedAccess = 4002;
        public const int UserRegistered = 4010;
        public const int UserRegistrationFailed = 4011;

        // File Operations (5000-5099)
        public const int FileUploadStarted = 5000;
        public const int FileUploadCompleted = 5001;
        public const int FileUploadFailed = 5002;
        public const int FileDeleted = 5010;

        // Validation & Business Logic Events (6000-6099)
        public const int ValidationFailed = 6000;
        public const int BusinessRuleViolation = 6001;

        // External API Calls (7000-7099)
        public const int ExternalApiCallStarted = 7000;
        public const int ExternalApiCallCompleted = 7001;
        public const int ExternalApiCallFailed = 7002;

        // Performance Metrics (8000-8099)
        public const int PerformanceMetricRecorded = 8000;
        public const int SlowOperationDetected = 8001;

        // Errors & Exceptions (9000-9099)
        public const int UnhandledException = 9000;
        public const int HandledException = 9001;
    }

    /// <summary>
    /// Scopes for grouping related log entries
    /// </summary>
    public static class Scopes
    {
        public const string HttpRequest = "HttpRequest";
        public const string DatabaseOperation = "DatabaseOperation";
        public const string ServiceOperation = "ServiceOperation";
        public const string UserContext = "UserContext";
        public const string TankContext = "TankContext";
        public const string PerformanceTracking = "PerformanceTracking";
    }

    /// <summary>
    /// Common property names for structured logging
    /// </summary>
    public static class Properties
    {
        public const string UserId = "UserId";
        public const string TankId = "TankId";
        public const string RequestId = "RequestId";
        public const string OperationName = "OperationName";
        public const string Duration = "Duration";
        public const string StatusCode = "StatusCode";
        public const string ExceptionType = "ExceptionType";
        public const string ItemCount = "ItemCount";
        public const string EntityType = "EntityType";
        public const string EntityId = "EntityId";
    }
}
