# MCP Trigger Function

This project is an Azure Functions application that implements the Model Context Protocol (MCP) to enable AI assistants to invoke functions directly from conversations.

## Features

- **Hello Tool**: A simple MCP tool that returns a greeting message.
- **Snippet Management Tools**: Save and retrieve code snippets.

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Azure Functions Core Tools
- Visual Studio or VS Code with Azure Functions extension

### Running Locally

1. Build the project using:
   ```
   dotnet build
   ```

2. Run the Azure Functions host:
   ```
   cd bin/Debug/net9.0
   func host start
   ```

   Alternatively, you can use the "MCP TOOL" task in VS Code.

3. The MCP server will be available at:
   ```
   http://localhost:7071/runtime/webhooks/mcp/sse
   ```

### MCP Tools

#### Hello Tool

A simple MCP tool that returns a greeting message.

```csharp
[Function(nameof(SayHello))]
public string SayHello(
    [McpToolTrigger(HelloToolName, HelloToolDescription)] ToolInvocationContext context
)
{
    logger.LogInformation("Saying hello");
    return "Hello I am your MCP Tool, how can I help you?";
}
```

## Deployment

### Adding MCP server to local VS Code

To configure VS Code to work with your local MCP function:

1. Create or edit the `.vscode/mcp.json` file in your project with the following content:
   ```json
   {
       "servers": {
           "local-mcp-function": {
               "type": "sse",
               "url": "http://localhost:7071/runtime/webhooks/mcp/sse"
           }
       }
   }
   ```

2. Start your function app locally using the "MCP TOOL" task in VS Code or run:
   ```
   cd MCPTriggerFunction
   dotnet build
   cd bin/Debug/net9.0
   func host start
   ```

3. Make sure VS Code's Copilot Chat is configured to use your local MCP server:
   - Open VS Code settings (Ctrl+,)
   - Search for "Copilot Chat MCP"
   - Ensure the MCP server settings point to your local server

4. Test the connection by asking Copilot Chat to use your MCP tool:
   ```
   say hello using mcp tool
   ```

### Deploy to Azure

1. Create an Azure Function App resource in Azure Portal
2. Deploy using VS Code Azure Functions extension or run:
   ```
   dotnet publish -c Release
   func azure functionapp publish <function-app-name>
   ```

3. Configure the MCP integration in your VS Code settings:
   ```json
   {
       "servers": {
           "remote-mcp-function": {
               "type": "sse",
               "url": "https://<functionapp-name>.azurewebsites.net/runtime/webhooks/mcp/sse",
               "headers": {
                   "x-functions-key": "<functions-mcp-extension-system-key>"
               }
           }
       }
   }
   ```

## Adding New MCP Tools

To add a new MCP tool:

1. Define the tool name and description in `ToolsInformation.cs`
2. Create a new class with a function method decorated with `[McpToolTrigger]`
3. Implement the tool's functionality in the method

Example for adding a weather tool:

```csharp
// In ToolsInformation.cs
public const string WeatherToolName = "weather";
public const string WeatherToolDescription = "Get current weather for a given location";

// New class for the weather tool
public class WeatherTool(ILogger<WeatherTool> logger)
{
    [Function(nameof(GetWeather))]
    public string GetWeather(
        [McpToolTrigger(WeatherToolName, WeatherToolDescription)] ToolInvocationContext context
    )
    {
        // Implement weather API call logic here
        return "Weather information...";
    }
}
```
