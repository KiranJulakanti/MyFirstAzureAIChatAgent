using MCPTriggerFunction.Services;
using MCPTriggerFunction.Services.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using static MCPTriggerFunction.ToolsInformation;

namespace MCPTriggerFunction
{
    public class TelemetryTool
    {
        private readonly ILogger<TelemetryTool> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly ApplicationInsightsQueryService _queryService;

        public TelemetryTool(
            ILogger<TelemetryTool> logger,
            ITelemetryService telemetryService,
            ApplicationInsightsQueryService queryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        [Function(nameof(GetApplicationErrors))]
        public async Task<string> GetApplicationErrors(
            [McpToolTrigger(GetErrorsToolName, GetErrorsToolDescription)] ToolInvocationContext context
        )
        {
            using var operation = _telemetryService.StartOperation("GetApplicationErrors");
            _telemetryService.TrackEvent("TelemetryTool.GetApplicationErrors.Started");
            
            try
            {
                // Set default values
                int hours = 24;
                int limit = 20;

                // Since we don't know the exact structure in this version of MCP,
                // let's use a minimal approach
                _logger.LogInformation("Getting application errors for the last {Hours} hours, limit: {Limit}", hours, limit);
                
                var errors = await _queryService.QueryErrorsAsync(hours, limit);
                
                _telemetryService.TrackEvent("TelemetryTool.GetApplicationErrors.Completed");
                return errors;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                _logger.LogError(ex, "Error retrieving application errors");
                return $"Error retrieving application errors: {ex.Message}";
            }
        }

        [Function(nameof(GetApplicationTraces))]
        public async Task<string> GetApplicationTraces(
            [McpToolTrigger(GetTracesToolName, GetTracesToolDescription)] ToolInvocationContext context
        )
        {
            using var operation = _telemetryService.StartOperation("GetApplicationTraces");
            _telemetryService.TrackEvent("TelemetryTool.GetApplicationTraces.Started");
            
            try
            {
                // Set default values
                string severityLevel = "";
                int hours = 24;
                int limit = 20;

                // Since we don't know the exact structure in this version of MCP,
                // let's use a minimal approach
                _logger.LogInformation("Getting application traces for the last {Hours} hours, severity: {SeverityLevel}, limit: {Limit}", 
                    hours, severityLevel, limit);
                
                var traces = await _queryService.QueryTracesAsync(severityLevel, hours, limit);
                
                _telemetryService.TrackEvent("TelemetryTool.GetApplicationTraces.Completed");
                return traces;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                _logger.LogError(ex, "Error retrieving application traces");
                return $"Error retrieving application traces: {ex.Message}";
            }
        }

        [Function(nameof(GetApplicationPerformance))]
        public async Task<string> GetApplicationPerformance(
            [McpToolTrigger(GetPerformanceToolName, GetPerformanceToolDescription)] ToolInvocationContext context
        )
        {
            using var operation = _telemetryService.StartOperation("GetApplicationPerformance");
            _telemetryService.TrackEvent("TelemetryTool.GetApplicationPerformance.Started");
            
            try
            {
                // Set default values
                int hours = 24;
                int limit = 20;

                // Since we don't know the exact structure in this version of MCP,
                // let's use a minimal approach
                _logger.LogInformation("Getting application performance metrics for the last {Hours} hours, limit: {Limit}", hours, limit);
                
                var performance = await _queryService.QueryPerformanceAsync(hours, limit);
                
                _telemetryService.TrackEvent("TelemetryTool.GetApplicationPerformance.Completed");
                return performance;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                _logger.LogError(ex, "Error retrieving application performance metrics");
                return $"Error retrieving application performance metrics: {ex.Message}";
            }
        }
    }
}
