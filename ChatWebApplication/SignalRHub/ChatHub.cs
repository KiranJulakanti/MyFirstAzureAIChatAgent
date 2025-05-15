using ChatWebApplication.AzureOpenAi;
using ChatWebApplication.Models;
using ChatWebApplication.SemanticKernel;
using ChatWebApplication.Services;
using ChatWebApplication.Services.Interfaces;
using ChatWebApplication.Services.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ChatWebApplication.SignalRHub
{
    public class ChatHub : Hub
    {
        private readonly ChatKernelPlugin _chatKernel;
        private readonly ILogger<ChatHub> _logger;
        private readonly ITelemetryService _telemetryService;
        private const string SystemUser = "System";

        private IBigCatService _bigCatService;
        private CaseService _caseService;

        public ChatHub(
            ChatKernelPlugin chatKernel,
            IBigCatService bigCatService,
            ILogger<ChatHub> logger,
            ITelemetryService telemetryService)
        {
            _chatKernel = chatKernel ?? throw new ArgumentNullException(nameof(chatKernel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));

            _bigCatService = bigCatService ?? throw new ArgumentNullException(nameof(bigCatService));
            _caseService = new CaseService(new HttpClient(), new CaseServiceSettings(), _telemetryService);
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            
            _telemetryService.TrackEvent("SignalR.ClientConnected", new Dictionary<string, string> {
                { "ConnectionId", connectionId }
            });
            
            await base.OnConnectedAsync();
            await SendSystemMessage("Welcome to the Chat Application! How can I assist you today?");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            
            if (exception != null)
            {
                _telemetryService.TrackException(exception, new Dictionary<string, string> {
                    { "ConnectionId", connectionId }
                });
            }
            
            _telemetryService.TrackEvent("SignalR.ClientDisconnected", new Dictionary<string, string> {
                { "ConnectionId", connectionId }
            });
            
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Processes an incoming message from a client, determines the intent, and sends an appropriate response.
        /// </summary>
        /// <param name="userInput">The raw user input for intent classification</param>
        /// <param name="message">The full message content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendMessage(string userInput, string message)
        {
            string connectionId = Context.ConnectionId;
            
            using var operation = _telemetryService.StartOperation("ProcessUserMessage", "SignalR");
            _telemetryService.SetProperty("ConnectionId", connectionId);
            _telemetryService.SetProperty("MessageLength", userInput?.Length.ToString() ?? "0");
            
            if (string.IsNullOrWhiteSpace(userInput))
            {
                _telemetryService.TrackTrace("Received empty message", SeverityLevel.Warning);
                await SendSystemMessage("I couldn't understand your message. Please try again.");
                return;
            }

            try
            {
                _telemetryService.TrackEvent("UserMessageReceived", new Dictionary<string, string> {
                    { "MessagePreview", userInput.Length > 20 ? userInput.Substring(0, 20) + "..." : userInput }
                });
                
                var stopwatch = Stopwatch.StartNew();
                var predictedIntent = await _chatKernel.GetUserIntent(userInput);
                stopwatch.Stop();
                
                _telemetryService.TrackEvent("IntentClassified", new Dictionary<string, string> {
                    { "Intent", predictedIntent },
                    { "ProcessingTimeMs", stopwatch.ElapsedMilliseconds.ToString() }
                });

                await RespondBasedOnIntent(predictedIntent, userInput);
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                await SendSystemMessage($"Exception occurred: {ex.Message}, Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Responds to user messages based on their determined intent.
        /// </summary>
        /// <param name="predictedIntent">The classified intent of the user message</param>
        /// <param name="message">The original user message</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task RespondBasedOnIntent(string predictedIntent, string userInput)
        {
            using var operation = _telemetryService.StartOperation($"Intent.{predictedIntent}", "IntentProcessing");
            
            _telemetryService.TrackTrace($"Processing intent: {predictedIntent}", SeverityLevel.Information, new Dictionary<string, string> {
                { "MessagePreview", userInput.Length > 50 ? userInput.Substring(0, 50) + "..." : userInput }
            });

            try
            {
                // Define a dictionary of intent handlers to make the code more maintainable
                var intentHandlers = new Dictionary<string, Func<string, Task>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["RecommendedProducts"] = async (userInput) =>
                    {
                        try
                        {
                            _telemetryService.TrackTrace("Handling RecommendedProducts intent", SeverityLevel.Information);
                            var startTime = DateTimeOffset.UtcNow;
                            var stopwatch = Stopwatch.StartNew();
                            
                            var bitCatProducts = await _bigCatService.GetProductDetails(); // this can be a Azure Search Service that calls any high dimensional database.
                            
                            stopwatch.Stop();
                            _telemetryService.TrackDependency("BigCatService", "GetProductDetails", "Products API Call", startTime, stopwatch.Elapsed, true);

                            await SendSystemMessage("I am working on pulling the product details for you, please hang on!.");

                            // NLM processing into conversational format
                            startTime = DateTimeOffset.UtcNow;
                            stopwatch.Restart();
                            var productDetails = await _chatKernel.FormatProductDetails(bitCatProducts);
                            stopwatch.Stop();
                            _telemetryService.TrackDependency("ChatKernel", "FormatProductDetails", "NLP Formatting", startTime, stopwatch.Elapsed, true);

                            await SendSystemMessage(productDetails);
                            await SendSystemMessage("Are you interested to purchase one or more of these product(s)?");
                        }
                        catch (Exception ex)
                        {
                            _telemetryService.TrackException(ex, new Dictionary<string, string> {
                                { "Intent", "RecommendedProducts" }
                            });
                            throw;
                        }
                    },

                    ["WantToPurchase"] = async (userInput) =>
                    {
                        _telemetryService.TrackEvent("PurchaseIntentDetected");
                        await SendSystemMessage($"Thank you for showing your interest.<br/>Before proceeding, wanted to know if you are a new customer to Microsoft?.");
                    },

                    ["NewCustomer"] = async (userInput) =>
                    {
                        _telemetryService.TrackEvent("NewCustomerIdentified");
                        await SendSystemMessage($"Great, in that case I would require your personal details to create an account with us.<br/>Are you interested to proceed? ");
                    },

                    ["ProvideDetails"] = async (userInput) =>
                    {
                        _telemetryService.TrackEvent("CustomerRequestedToProvideDetails");
                        await SendSystemMessage($"Please <a href='#' id='ProvideDetails' onclick='ProvideDetails()'>click here</a> to provide your details.");
                    },

                    ["DetailsReceived"] = async (userInput) =>
                    {
                        await SendSystemMessage($"Ok, I received your details, I am in the process of creating your account with us, <br/> please hang on a few mins while we setup your account.");

                        try
                        {
                            var customerDetails = JsonConvert.DeserializeObject<CustomerDetails>(userInput);
                            _telemetryService.TrackEvent("CustomerDetailsReceived", new Dictionary<string, string> {
                                { "CustomerName", customerDetails.CustomerName }
                                // Don't log CustomerTaxId as it might contain sensitive information
                            });
                            
                            var startTime = DateTimeOffset.UtcNow;
                            var stopwatch = Stopwatch.StartNew();
                            
                            var apiResponse = await _caseService.CreateCustomerAccount(customerDetails.CustomerName, customerDetails.CustomerTaxId);
                            
                            stopwatch.Stop();
                            _telemetryService.TrackDependency("CaseService", "CreateCustomerAccount", "Account Creation API Call", startTime, stopwatch.Elapsed, !string.IsNullOrEmpty(apiResponse));
                            
                            await SendSystemMessage($"Great news! Your account has been successfully set up. Please save this CustomerAccountId: {apiResponse} for future reference in our communications.");
                            
                            _telemetryService.TrackEvent("CustomerAccountCreated", new Dictionary<string, string> {
                                { "AccountId", apiResponse }
                            });
                        }
                        catch (Exception ex)
                        {
                            _telemetryService.TrackException(ex, new Dictionary<string, string> {
                                { "Intent", "DetailsReceived" }
                            });
                            throw;
                        }
                    },

                    ["Unknown"] = async (userInput) =>
                    {
                        _telemetryService.TrackEvent("UnknownIntentDetected");
                        await SendSystemMessage($"Unknown intent received.");
                    }
                };

                // Check if we have a handler for this intent
                if (intentHandlers.TryGetValue(predictedIntent, out var handler))
                {
                    await handler(userInput);
                }
                else
                {
                    _telemetryService.TrackTrace("No handler found for intent", SeverityLevel.Warning, new Dictionary<string, string> {
                        { "Intent", predictedIntent }
                    });
                    await SendSystemMessage("I'm not sure how to respond to that. Could you rephrase your question?");
                }
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// Sends a system message to all connected SignalR clients.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendSystemMessage(string message)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveMessage", SystemUser, message);
                _telemetryService.TrackTrace("System message sent", SeverityLevel.Information, new Dictionary<string, string> {
                    { "MessagePreview", message.Length > 50 ? message.Substring(0, 50) + "..." : message }
                });
            }
            catch (Exception ex)
            {
                _telemetryService.TrackException(ex, new Dictionary<string, string> {
                    { "MessageType", "SystemMessage" }
                });
                throw;
            }
        }
    }

    public class CustomerDetails
    {
        public string CustomerName { get; set; }
        public string CustomerTaxId { get; set; }
    }
}
