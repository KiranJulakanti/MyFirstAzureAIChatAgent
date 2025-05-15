namespace ChatWebApplication
{
    /// <summary>
    /// Provides standardized prompt templates for various AI tasks
    /// </summary>
    public class PromptTemplateProvider
    {
        /// <summary>
        /// Gets the prompt template for intent classification
        /// </summary>
        public static string GetIntentClassifierPrompt
        {
            get 
            { 
                return @"
                    You are an intent classifier. Classify the user's intent into one of the following categories:
                    - RecommendedProducts: User is looking for product recommendations.
                    - CreateAccount: User wants to create or generate an account.
                    - WantToPurchase: User wants to purchase or buy the products, or is interested in buying or saying yes to purchase.
                    - NewCustomer: Yes, user says he is a new customer.
                    - ProvideDetails: Yes, user says he wanted to provide personal details.
                    - DetailsReceived: User sends his details in as CustomerName and CustomerTaxId (or) Name: and TaxId:.
                    - Unknown: User's intent doesn't match any of the above categories.

                    User message: {{$userInput}}

                    Return ONLY ONE of these exact intent names without any explanation or additional text.";
            }
        }

        /// <summary>
        /// Gets the prompt template for product full details
        /// </summary>
        public static string GetProductDetailsPrompt
        {
            get
            {
                return @"Give me top 5 Microsoft Surface Products.
                    - Include product specifications processor type, RAM, Battery, Price only.
                    - Do not include any additional content.";
            }
        }

        /// <summary>
        /// Gets the prompt template for product details
        /// </summary>
        public static string FormatProductDetailsPrompt 
        { 
            get
            {  
                return @"
                    Your goal is to concise and format the Product details given to you.
                    - Do not include any specifications or details about the product.
                    - Do not add any other products other than the ones given to you.
                    - Return only the product name, processor, and price.
                    - Return only top three products by price in descending order.
                    - Do not include any additional content.

                    Here are the product details: '{{$products}}'
                    Format the output as follows:
                        Here are some popular Microsoft Hardware Products, <br/>
                        1. Mircrosoft Surface Pro - I7 - $1500 <br/>
                        2. Mircrosoft Surface Pro V1 - I5 - $1000"; 
            }
        }
    }
}
