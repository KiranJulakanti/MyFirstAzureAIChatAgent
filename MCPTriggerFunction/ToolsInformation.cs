using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCPTriggerFunction
{
    internal sealed class ToolsInformation
    {
        public const string SaveSnippetToolName = "save_snippet";
        public const string SaveSnippetToolDescription =
            "Saves a code snippet into your snippet collection.";
        public const string GetSnippetToolName = "get_snippets";
        public const string GetSnippetToolDescription =
            "Gets code snippets from your snippet collection.";
        public const string SnippetNamePropertyName = "snippetname";
        public const string SnippetPropertyName = "snippet";
        public const string SnippetNamePropertyDescription = "The name of the snippet.";
        public const string SnippetPropertyDescription = "The code snippet.";
        public const string PropertyType = "string";
        public const string HelloToolName = "hello";
        public const string HelloToolDescription =
            "Simple hello world MCP Tool that responses with a hello message.";
            
        // Telemetry tool definitions
        public const string GetErrorsToolName = "get_app_errors";
        public const string GetErrorsToolDescription = "Gets error logs from Application Insights for the application.";
        public const string GetTracesToolName = "get_app_traces"; 
        public const string GetTracesToolDescription = "Gets trace logs from Application Insights for the application.";
        public const string GetPerformanceToolName = "get_app_performance";
        public const string GetPerformanceToolDescription = "Gets performance metrics from Application Insights for the application.";
            
        // Telemetry tool parameter definitions
        public const string HoursPropertyName = "hours";
        public const string HoursPropertyDescription = "Number of hours to look back for logs (default: 24).";
        public const string LimitPropertyName = "limit";
        public const string LimitPropertyDescription = "Maximum number of records to return (default: 20).";
        public const string SeverityLevelPropertyName = "severityLevel";
        public const string SeverityLevelPropertyDescription = "Filter traces by severity level (Verbose, Information, Warning, Error, Critical). Leave empty for all levels.";
    }
}
