using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// Configure the application to use environment variables for sensitive data
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// Main program execution
await RunChatAsync();

// Main chat loop function
async Task RunChatAsync()
{
    Console.WriteLine("=== Azure OpenAI Chat Agent ===");
    Console.WriteLine("Type 'exit' to quit the application");
    Console.WriteLine();

    // Get the Azure OpenAI client
    var client = GetOpenAIClient();
    if (client == null)
    {
        Console.WriteLine("Failed to initialize Azure OpenAI client. Please check your configuration.");
        return;
    }

    // Get the deployment name from environment variables
    string deploymentName = "kijula-gpt-4o";
    
    if (string.IsNullOrEmpty(deploymentName))
    {
        Console.WriteLine("Error: AZURE_OPENAI_DEPLOYMENT_NAME environment variable is not set.");
        Console.WriteLine("Please set it to a valid deployment name from your Azure OpenAI resource.");
        return;
    }
    
    Console.WriteLine($"Using deployment: {deploymentName}");
    
    // Initialize chat history with system message
    var chatHistory = new List<ChatRequestMessage>
    {
        new ChatRequestSystemMessage("You are a helpful AI assistant that provides concise and accurate information.")
    };

    try
    {
        while (true)
        {
            // Get user input
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nUser: ");
            Console.ResetColor();
            
            string? userInput = Console.ReadLine();
            
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
                
                Console.WriteLine("Thinking...");
                
                // Get response from Azure OpenAI
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionOptions);
                
                // Extract and display the response
                string assistantResponse = response.Value.Choices[0].Message.Content;
                
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Assistant: ");
                Console.ResetColor();
                Console.WriteLine(assistantResponse);
                
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
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"An error occurred: {ex.Message}");
        Console.ResetColor();
    }
    
    Console.WriteLine("Thank you for using Azure OpenAI Chat Agent! Goodbye.");
}

// Function to create and configure the Azure OpenAI client
OpenAIClient? GetOpenAIClient()
{
    try
    {
        // Get configuration from environment variables
        string? endpoint = "https://kijulaaiservice.openai.azure.com/";
        string? apiKey = "BA3VzQk7Pn5Lo9gHL8ixOstCASR6DSU4sYMpTiqUrDIzNhCXar83JQQJ99BDAC4f1cMXJ3w3AAAAACOGNSvE";
        bool useManagedIdentity = string.IsNullOrEmpty(apiKey);

        // Validate configuration
        if (string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("Error: Azure OpenAI endpoint not configured.");
            Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable to your Azure OpenAI endpoint URL.");
            Console.WriteLine("Example: https://yourservice.openai.azure.com/");
            return null;
        }

        OpenAIClient client;

        // Create client with either API key (for local development) or DefaultAzureCredential (for deployed environments)
        if (!useManagedIdentity)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("Error: Azure OpenAI API key not configured.");
                Console.WriteLine("Please set the AZURE_OPENAI_API_KEY environment variable to your Azure OpenAI API key.");
                return null;
            }
            
            // Using API key authentication (for development)
            Console.WriteLine("Using API key authentication");
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
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed to create OpenAI client: {ex.Message}");
        Console.ResetColor();
        return null;
    }
}
