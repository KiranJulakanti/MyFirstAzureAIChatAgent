using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using ChatWebApplication.Settings;

namespace ChatWebApplication.AzureOpenAi
{
    public class AzureOpenAIService
    {
        public async Task<string> RunChatAsync(string userInput)
        {
            // Get the Azure OpenAI client
            var client = GetOpenAIClient();
            
            if (client == null)
            {
                throw new Exception("Failed to initialize Azure OpenAI client. Please check your configuration.");
            }

            // Get the deployment name from environment variables
            string deploymentName = AzureOpenAiSettings.DeploymentName;

            if (string.IsNullOrEmpty(deploymentName))
            {
                throw new Exception("Please set it to a valid deployment name from your Azure OpenAI resource.");
            }

            // Initialize chat history with system message
            var chatHistory = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are a helpful AI assistant that provides concise and accurate information.")
            };

            try
            {
                while (true)
                {
                    // Exit if user types 'exit'
                    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
                    {
                        break;
                    }

                    // Add user message to history
                    chatHistory.Add(new ChatRequestUserMessage(userInput));

                    try
                    {
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

                        // Get response from Azure OpenAI
                        Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionOptions);

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

                        return assistantResponse;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return "no response returned from agent";
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
                    Console.WriteLine("Error: Azure OpenAI endpoint not configured.");
                    return null;
                }

                OpenAIClient client;

                // Create client with either API key (for local development) or DefaultAzureCredential (for deployed environments)
                if (!useManagedIdentity)
                {
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        Console.WriteLine("Error: Azure OpenAI API key not configured.");
                        return null;
                    }

                    client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                }
                else
                {
                    // Using Managed Identity (recommended for production)
                    Console.WriteLine("Using Managed Identity authentication");
                    client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
                }

                return client;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
