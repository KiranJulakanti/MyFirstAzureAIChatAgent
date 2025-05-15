using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using ChatWebApplication.Services.Telemetry;
using ChatWebApplication.Settings;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace ChatWebApplication.AzureOpenAi
{
    public class AzureOpenAIService
    {
        private readonly ITelemetryService _telemetryService;

        public AzureOpenAIService(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        public async Task<string> RunChatAsync(string userInput)
        {
            // Track the operation with Application Insights
            using var operation = _telemetryService.StartOperation("RunChatAsync", "AzureOpenAI");
            
            try
            {
                _telemetryService.SetProperty("UserInputLength", userInput.Length.ToString());
                
                // Get the Azure OpenAI client
                var client = GetOpenAIClient();
                
                if (client == null)
                {
                    var exception = new Exception("Failed to initialize Azure OpenAI client. Please check your configuration.");
                    _telemetryService.TrackException(exception);
                    throw exception;
                }

                // Get the deployment name from environment variables
                string deploymentName = AzureOpenAiSettings.DeploymentName;

                if (string.IsNullOrEmpty(deploymentName))
                {
                    var exception = new Exception("Please set it to a valid deployment name from your Azure OpenAI resource.");
                    _telemetryService.TrackException(exception);
                    throw exception;
                }

                // Initialize chat history with system message
                var chatHistory = new List<ChatRequestMessage>
                {
                    new ChatRequestSystemMessage("You are a helpful AI assistant that provides concise and accurate information.")
                };

                // Exit if user types 'exit'
                if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
                {
                    _telemetryService.TrackTrace("User exited the chat", SeverityLevel.Information);
                    return string.Empty;
                }

                // Add user message to history
                chatHistory.Add(new ChatRequestUserMessage(userInput));

                // Create chat completion options
                var chatCompletionOptions = new ChatCompletionsOptions
                {
                    DeploymentName = deploymentName,
                    Messages = { }
                };

                // Add all messages from chat history
                foreach (var message in chatHistory)
                {
                    chatCompletionOptions.Messages.Add(message);
                }

                // Measure the dependency call to Azure OpenAI
                var stopwatch = Stopwatch.StartNew();
                var startTime = DateTimeOffset.UtcNow;
                
                try
                {
                    _telemetryService.TrackTrace($"Sending request to Azure OpenAI deployment: {deploymentName}", SeverityLevel.Information);
                    
                    // Get response from Azure OpenAI
                    Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionOptions);

                    stopwatch.Stop();
                    
                    // Track the successful dependency call
                    _telemetryService.TrackDependency(
                        "AzureOpenAI",
                        $"ChatCompletion/{deploymentName}",
                        $"Model: {AzureOpenAiSettings.ModelId}",
                        startTime,
                        stopwatch.Elapsed,
                        true);

                    // Extract and display the response
                    string assistantResponse = response.Value.Choices[0].Message.Content;

                    // Add assistant response to chat history
                    chatHistory.Add(new ChatRequestAssistantMessage(assistantResponse));

                    // Limit chat history size (optional)
                    if (chatHistory.Count > 10)
                    {
                        // Keep system message and last 9 messages
                        chatHistory = new List<ChatRequestMessage>
                        {
                            chatHistory[0] // System message
                        };
                        chatHistory.AddRange(chatHistory.GetRange(chatHistory.Count - 9, 9));
                    }

                    // Track token usage metrics if available
                    if (response.Value.Usage != null)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "PromptTokens", response.Value.Usage.PromptTokens.ToString() },
                            { "CompletionTokens", response.Value.Usage.CompletionTokens.ToString() },
                            { "TotalTokens", response.Value.Usage.TotalTokens.ToString() }
                        };
                        
                        _telemetryService.TrackEvent("AzureOpenAI.TokenUsage", properties);
                    }

                    return assistantResponse;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    
                    // Track the failed dependency call
                    _telemetryService.TrackDependency(
                        "AzureOpenAI",
                        $"ChatCompletion/{deploymentName}",
                        $"Model: {AzureOpenAiSettings.ModelId}",
                        startTime,
                        stopwatch.Elapsed,
                        false);
                    
                    _telemetryService.TrackException(ex, new Dictionary<string, string> {
                        { "DeploymentName", deploymentName },
                        { "ModelId", AzureOpenAiSettings.ModelId }
                    });
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                throw;
            }
        }

        // Function to create and configure the Azure OpenAI client
        public OpenAIClient GetOpenAIClient()
        {
            try
            {
                string? endpoint = AzureOpenAiSettings.Endpoint;
                string? apiKey = AzureOpenAiSettings.ApiKey;

                bool useManagedIdentity = string.IsNullOrEmpty(apiKey);

                // Validate configuration
                if (string.IsNullOrEmpty(endpoint))
                {
                    _telemetryService.TrackTrace("Azure OpenAI endpoint not configured", SeverityLevel.Error);
                    return null;
                }

                OpenAIClient client;

                // Create client with either API key (for local development) or DefaultAzureCredential (for deployed environments)
                if (!useManagedIdentity)
                {
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        _telemetryService.TrackTrace("Azure OpenAI API key not configured", SeverityLevel.Error);
                        return null;
                    }

                    _telemetryService.TrackTrace("Using API Key authentication", SeverityLevel.Information);
                    client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                }
                else
                {
                    // Using Managed Identity (recommended for production)
                    _telemetryService.TrackTrace("Using Managed Identity authentication", SeverityLevel.Information);
                    client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
                }

                return client;
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                throw;
            }
        }
    }
}
