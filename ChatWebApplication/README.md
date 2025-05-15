# Azure AI Chat Application

## Overview
This application is an intelligent chat interface built on Azure OpenAI services, SignalR, and Microsoft Semantic Kernel. It provides conversational AI capabilities that can understand user intents, process natural language, and interact with various services to provide responses.

## Technology Stack

### Frontend
- ASP.NET Core Razor Pages
- SignalR for real-time communication
- HTML/CSS/JavaScript

### Backend
- .NET 8.0
- Azure OpenAI for natural language processing
- Microsoft Semantic Kernel (version 1.0.0-rc3) for AI orchestration
- SignalR for real-time server-client communication
- Application Insights for telemetry

### Azure Services
- Azure OpenAI Service
- Azure Application Insights
- Azure Identity for authentication

## Architecture

The application follows a service-oriented architecture with the following key components:

### Core Components

1. **ChatHub (SignalR)**: Manages real-time communication between clients and server
2. **ChatKernelPlugin**: Integrates with Semantic Kernel to provide AI capabilities
3. **AzureOpenAIService**: Handles communication with Azure OpenAI
4. **BigCatService**: External service integration
5. **CaseService**: Manages case-related operations
6. **TelemetryService**: Application Insights integration for logging and monitoring

### AI Features

- Intent recognition for understanding user requests
- Product recommendation capabilities
- Customer account creation flow
- Natural language processing for various use cases

## Telemetry and Application Insights Integration

This application uses Azure Application Insights for comprehensive monitoring and diagnostics. The implementation follows these patterns:

### Core Telemetry Components

1. **ITelemetryService Interface**: Provides a standardized way to log events, exceptions, traces, and dependencies across the application.
2. **ApplicationInsightsTelemetryService**: Implementation of the ITelemetryService interface that uses Application Insights.

### Key Telemetry Features

- **Distributed Tracing**: Track operations across service boundaries
- **Dependency Tracking**: Monitor calls to external services like Azure OpenAI
- **Exception Monitoring**: Capture and log exceptions with context
- **Performance Metrics**: Track operation durations and response times
- **Custom Events**: Log business-specific events for analytics
- **User Flows**: Track user interactions and conversation flows

### Instrumented Services

- **ChatHub**: Tracks connection events and message processing
- **AzureOpenAIService**: Monitors API calls to Azure OpenAI, including token usage
- **ChatKernelPlugin**: Tracks intent classification and other AI operations
- **BigCatService**: Monitors product detail retrieval operations
- **CaseService**: Tracks customer account creation flows

### Using Application Insights

The Application Insights dashboard provides:

1. **Live Metrics**: Real-time monitoring of application performance
2. **Transaction Search**: Find and inspect specific user interactions
3. **End-to-End Transaction Details**: View complete trace of operations across services
4. **Failure Analysis**: Identify and diagnose errors
5. **User Flow Analysis**: Understand conversation patterns

## Setup and Configuration

### Prerequisites
- .NET 8.0 SDK
- Azure account with Azure OpenAI service configured
- Azure Application Insights resource
- Visual Studio 2022 or later (recommended)

### Environment Configuration
Configure the following in your appsettings.json:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "your-application-insights-connection-string"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  },
  "AzureOpenAI": {
    "Endpoint": "your-azure-openai-endpoint",
    "Key": "your-azure-openai-key",
    "DeploymentName": "your-deployment-name",
    "ModelId": "your-model-id"
  }
}
```

## Running the Application

1. Clone the repository
2. Configure environment variables in appsettings.json
3. Restore NuGet packages
4. Build and run the application

```bash
dotnet restore
dotnet build
dotnet run
```

## Debugging with Application Insights

### Local Debugging
1. Set a breakpoint in any service
2. Run the application in debug mode
3. Monitor Application Insights live stream for real-time telemetry

### Production Monitoring
1. Deploy the application to Azure
2. Open the Application Insights resource in the Azure portal
3. Use Transaction Search to find and inspect user interactions
4. Use the Application Map to visualize service dependencies

### Common Queries

#### Find failed operations
```kusto
exceptions
| where timestamp > ago(24h)
| order by timestamp desc
```

#### Track Azure OpenAI usage
```kusto
customEvents
| where name == "AzureOpenAI.TokenUsage"
| extend promptTokens = todouble(customDimensions.PromptTokens), 
         completionTokens = todouble(customDimensions.CompletionTokens),
         totalTokens = todouble(customDimensions.TotalTokens)
| summarize avg(promptTokens), avg(completionTokens), avg(totalTokens), count() by bin(timestamp, 1h)
| render timechart
```

#### Monitor service performance
```kusto
dependencies
| where timestamp > ago(24h)
| summarize avgDuration=avg(duration), count=count() by target
| order by avgDuration desc
```

## Best Practices

### Security
- Store sensitive information like connection strings in Azure Key Vault
- Use Managed Identity for secure Azure service access
- Implement proper authentication and authorization

### Performance
- Use adaptive sampling in Application Insights to control telemetry volume
- Implement batching for high-volume scenarios
- Use appropriate sampling rates based on traffic

### Monitoring
- Set up alerts for critical exceptions
- Monitor token usage to control costs
- Track conversation success rates

## Additional Resources

- [Azure Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Microsoft Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/overview/)
