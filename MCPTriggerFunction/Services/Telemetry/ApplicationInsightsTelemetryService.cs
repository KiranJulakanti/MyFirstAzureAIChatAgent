using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace MCPTriggerFunction.Services.Telemetry
{
    public interface ITelemetryService
    {
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null);
        void TrackException(Exception exception, IDictionary<string, string>? properties = null);
        void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
        IDisposable StartOperation(string operationName, string operationType = "Custom");
        void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string>? properties = null);
        void SetProperty(string key, string value);
    }

    public class ApplicationInsightsTelemetryService : ITelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ApplicationInsightsTelemetryService> _logger;

        public ApplicationInsightsTelemetryService(
            TelemetryClient telemetryClient, 
            ILogger<ApplicationInsightsTelemetryService> logger)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
        {
            _telemetryClient.TrackEvent(eventName, properties);
            _logger.LogInformation("Event tracked: {EventName}", eventName);
        }

        public void TrackException(Exception exception, IDictionary<string, string>? properties = null)
        {
            _telemetryClient.TrackException(exception, properties);
            _logger.LogError(exception, "Exception tracked");
        }

        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            var telemetry = new DependencyTelemetry
            {
                Type = dependencyTypeName,
                Name = dependencyName,
                Data = data,
                Timestamp = startTime,
                Duration = duration,
                Success = success
            };

            _telemetryClient.TrackDependency(telemetry);
            
            if (success)
                _logger.LogInformation("Dependency call to {DependencyName} completed in {Duration}ms", dependencyName, duration.TotalMilliseconds);
            else
                _logger.LogWarning("Dependency call to {DependencyName} failed after {Duration}ms", dependencyName, duration.TotalMilliseconds);
        }

        public IDisposable StartOperation(string operationName, string operationType = "Custom")
        {
            // Create operation with ActivitySource to ensure proper distributed tracing
            var operation = _telemetryClient.StartOperation<RequestTelemetry>(operationName);
            
            _logger.LogInformation("Operation started: {OperationName}", operationName);
            
            return operation;
        }

        public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string>? properties = null)
        {
            _telemetryClient.TrackTrace(message, severityLevel, properties);
            
            // Log to ILogger with appropriate level based on SeverityLevel
            switch (severityLevel)
            {
                case SeverityLevel.Verbose:
                    _logger.LogTrace(message);
                    break;
                case SeverityLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case SeverityLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case SeverityLevel.Error:
                    _logger.LogError(message);
                    break;
                case SeverityLevel.Critical:
                    _logger.LogCritical(message);
                    break;
                default:
                    _logger.LogInformation(message);
                    break;
            }
        }

        public void SetProperty(string key, string value)
        {
            Activity.Current?.SetTag(key, value);
            _logger.LogDebug("Set property {Key}={Value} on current operation", key, value);
        }
    }
}
