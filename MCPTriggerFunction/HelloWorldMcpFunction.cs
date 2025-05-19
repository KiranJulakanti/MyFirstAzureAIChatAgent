using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using System.Text.Json;

using static MCPTriggerFunction.ToolsInformation;


namespace MCPTriggerFunction
{
    public class HelloTool(ILogger<HelloTool> logger)
    {
        [Function(nameof(SayHello))]
        public string SayHello(
            [McpToolTrigger(HelloToolName, HelloToolDescription)] ToolInvocationContext context
        )
        {
            logger.LogInformation("Saying hello");
            return "Hello I am your MCP Tool, how can I help you?";
        }

        [Function(nameof(ReportWeather))]
        public string ReportWeather(
            [McpToolTrigger("ReportWeather", "You provide weather details about your city.")] ToolInvocationContext context
        )
        {
            logger.LogInformation("Saying hello");
            return "Hello I am your MCP Tool, current weather data is not availble?";
        }
    }
}
