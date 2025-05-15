using ChatWebApplication.AzureOpenAi;
using ChatWebApplication.Models;
using ChatWebApplication.SemanticKernel;
using ChatWebApplication.Services;
using ChatWebApplication.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ChatWebApplication.SignalRHub
{
    public class ChatHub : Hub
    {
        private readonly ChatKernelPlugin _chatKernel;
        private readonly ILogger<ChatHub> _logger;
        private const string SystemUser = "System";

        private IBigCatService _bigCatService;
        private CaseService _caseService;

        public ChatHub(ChatKernelPlugin chatKernel,
            IBigCatService bigCatService,
            ILogger<ChatHub> logger)
        {
            _chatKernel = chatKernel ?? throw new ArgumentNullException(nameof(chatKernel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _bigCatService = bigCatService ?? throw new ArgumentNullException(nameof(bigCatService));
            _caseService = new CaseService(new HttpClient(), new CaseServiceSettings());
        }

        /// <summary>
        /// Processes an incoming message from a client, determines the intent, and sends an appropriate response.
        /// </summary>
        /// <param name="userInput">The raw user input for intent classification</param>
        /// <param name="message">The full message content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendMessage(string userInput, string message)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                await SendSystemMessage("I couldn't understand your message. Please try again.");
                return;
            }

            try
            {
                var predictedIntent = await _chatKernel.GetUserIntent(userInput);

                await RespondBasedOnIntent(predictedIntent, userInput);
            }
            catch (Exception ex)
            {
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
            _logger.LogInformation("Processing intent: {Intent} for message: {MessagePreview}",
                predictedIntent, userInput.Length > 50 ? userInput.Substring(0, 50) + "..." : userInput);

            try
            {
                // Define a dictionary of intent handlers to make the code more maintainable
                var intentHandlers = new Dictionary<string, Func<string, Task>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["RecommendedProducts"] = async (userInput) =>
                    {
                        try
                        {
                            //_bigCatService = new BigCatService(new HttpClient(), new BigCatSettings());
                            //var products = await _bigCatService.GetProductDetails("1E4-00005", "7H6K", "SG", "en-SG", "correlationId");
                            
                            var bitCatProducts = await _bigCatService.GetProductDetails(); // this can be a Azure Search Service that calls any high dimensional database.

                            await SendSystemMessage("I am working on pulling the product details for you, please hang on!.");

                            // NLM processing into conversational format
                            var productDetails = await _chatKernel.FormatProductDetails(bitCatProducts);

                            await SendSystemMessage(productDetails);
                            await SendSystemMessage("Are you interested to purchase one or more of these product(s)?");
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    },

                    ["WantToPurchase"] = async (userInput) =>
                    {
                        await SendSystemMessage($"Thank you for showing your interest.<br/>Before proceeding, wanted to know if you are a new customer to Microsoft?.");
                    },

                    ["NewCustomer"] = async (userInput) =>
                    {
                        await SendSystemMessage($"Great, in that case I would require your personal details to create an account with us.<br/>Are you interested to proceed? ");
                    },

                    ["ProvideDetails"] = async (userInput) =>
                    {
                        await SendSystemMessage($"Please <a href='#' id='ProvideDetails' onclick='ProvideDetails()'>click here</a> to provide your details.");
                    },

                    ["DetailsReceived"] = async (userInput) =>
                    {
                        await SendSystemMessage($"Ok, I received your details, I am in the process of creating your account with us, <br/> please hang on a few mins while we setup your account.");

                        try
                        {
                            var customerDetails = JsonConvert.DeserializeObject<CustomerDetails>(userInput);
                            
                            var apiResponse = await _caseService.CreateCustomerAccount(customerDetails.CustomerName, customerDetails.CustomerTaxId);
                            
                            await SendSystemMessage($"Great news! Your account has been successfully set up. Please save this CustomerAccountId: {apiResponse} for future reference in our communications.");
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    },

                    ["Unknown"] = async (userInput) =>
                    {
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
                    await SendSystemMessage("I'm not sure how to respond to that. Could you rephrase your question?");
                }
            }
            catch (Exception ex)
            {
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
            await Clients.All.SendAsync("ReceiveMessage", SystemUser, message);
        }
    }

    public class CustomerDetails
    {
        public string CustomerName { get; set; }
        public string CustomerTaxId { get; set; }
    }
}
