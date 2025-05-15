using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace ChatWebApplication.Services.Telemetry
{
    public interface ITelemetryService
    {
        /// <summary>
        /// Track a custom event with optional properties
        /// </summary>
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null);

        /// <summary>
        /// Track an exception with optional properties
        /// </summary>
        void TrackException(Exception exception, IDictionary<string, string>? properties = null);

        /// <summary>
        /// Track a dependency call (e.g., to Azure OpenAI, external APIs)
        /// </summary>
        void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
        
        /// <summary>
        /// Start an operation (creates a new Activity and RequestTelemetry)
        /// </summary>
        IDisposable StartOperation(string operationName, string operationType = "Custom");
        
        /// <summary>
        /// Track a trace message with optional properties
        /// </summary>
        void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string>? properties = null);
        
        /// <summary>
        /// Sets a property on the current operation
        /// </summary>
        void SetProperty(string key, string value);
    }
}