# Publishing Your ChatWebApplication Agent to Copilot.microsoft.com

This guide provides step-by-step instructions for publishing your ChatWebApplication agent to Microsoft Copilot.

## Step 1: Prepare Your Agent Code

1. First, ensure your agent code is ready for deployment:
   - Make sure all required configurations in `appsettings.json` are properly set up.
   - Verify that your agent is working locally.

## Step 2: Register in the Copilot Plugins Program

1. Visit the Microsoft Copilot Plugin Developer Portal at https://aka.ms/copilot-plugins
2. Sign in with your Microsoft account.
3. Register for the Copilot Plugins Program if you haven't already.

## Step 3: Prepare Your Azure Environment

1. Create an Azure Function App to host your MCP (Model Context Protocol) functions:
   ```powershell
   az login
   az group create --name YourResourceGroup --location EastUS
   az storage account create --name yourstoragename --location EastUS --resource-group YourResourceGroup --sku Standard_LRS
   az functionapp create --name YourMCPFunctionApp --storage-account yourstoragename --consumption-plan-location EastUS --resource-group YourResourceGroup --functions-version 4 --runtime dotnet-isolated --os-type Windows
   ```

2. Configure your Azure Function App:
   ```powershell
   az functionapp config appsettings set --name YourMCPFunctionApp --resource-group YourResourceGroup --settings "AzureOpenAI:Endpoint=your-azure-openai-endpoint" "AzureOpenAI:Key=your-azure-openai-key" "AzureOpenAI:DeploymentName=your-deployment-name" "AzureOpenAI:ModelId=your-model-id" 
   ```

## Step 4: Deploy Your MCP Function App

1. Navigate to your MCPTriggerFunction project:
   ```powershell
   cd c:\jkiran\code\Learning\AzureAISkillFest\AzureAIChatAgent\MCPTriggerFunction
   ```

2. Publish your function to Azure:
   ```powershell
   dotnet publish -c Release
   cd bin\Release\net9.0\publish
   func azure functionapp publish YourMCPFunctionApp
   ```

   Alternatively, use the VS Code tasks provided in your project:
   ```powershell
   # These commands run the VS Code tasks defined in your project
   # First clean and build the function project
   dotnet clean --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
   dotnet publish --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
   ```

3. Note your MCP endpoint URL, which will be in the format:
   ```
   https://YourMCPFunctionApp.azurewebsites.net/runtime/webhooks/mcp/sse
   ```

## Step 5: Create a Manifest for Your Plugin

1. Create a manifest JSON file named `manifest.json` with the following content:

```json
{
  "schema_version": "v1",
  "name_for_human": "Your Agent Name",
  "name_for_model": "your_agent_name",
  "description_for_human": "A brief description of what your agent does for users",
  "description_for_model": "A more detailed description for the model explaining your agent's capabilities",
  "auth": {
    "type": "none"
  },
  "api": {
    "type": "mcp",
    "url": "https://YourMCPFunctionApp.azurewebsites.net/runtime/webhooks/mcp/sse"
  },
  "logo_url": "https://yourdomain.com/logo.png",
  "contact_email": "your-email@example.com",
  "legal_info_url": "https://yourdomain.com/legal"
}
```

2. Save this file in a location you can access.

## Step 6: Submit Your Plugin to the Copilot Plugin Store

1. Go to the Microsoft Copilot Plugin Developer Portal at https://aka.ms/copilot-plugins
2. Click on "Submit a Plugin" or a similar option.
3. Fill out the submission form:
   - Upload your `manifest.json` file
   - Provide any additional information requested
   - Select the appropriate category for your plugin
   - Agree to the terms and conditions

## Step 7: Test Your Plugin in Copilot Preview

1. After submission, you may be granted access to test your plugin in a preview environment.
2. To test your plugin:
   - Open Microsoft Copilot (https://copilot.microsoft.com)
   - Go to Settings > Plugins
   - Enable your plugin in the "Developer Plugins" section if available

## Step 8: Monitor and Update

1. Set up monitoring for your Azure Function App:
   ```powershell
   az monitor diagnostic-settings create --resource YourMCPFunctionApp --name mydiagnosticsettings --resource-group YourResourceGroup --logs "[{\"category\":\"FunctionAppLogs\",\"enabled\":true}]" --metrics "[{\"category\":\"AllMetrics\",\"enabled\":true}]" --workspace YourLogAnalyticsWorkspace
   ```

2. Monitor your application using Application Insights, which is already configured in your project.

## Additional Notes

- Your application already includes telemetry tools using Application Insights, which will help you monitor the usage and performance of your agent.
- The MCP (Model Context Protocol) is the standard protocol used for Copilot plugins, and your project already implements it.
- Remember to periodically update your agent as Copilot's capabilities evolve.
- If your agent requires authentication, you'll need to modify the manifest.json and implement the appropriate authentication flow.

## Troubleshooting

- If your plugin isn't appearing in the Copilot plugin store, verify that your Azure Function App is accessible and that the MCP endpoint URL is correct.
- Check the Application Insights logs for any errors or issues.
- Ensure that your plugin complies with Microsoft's content policies and guidelines.

## References

- [Microsoft Copilot Plugin Documentation](https://aka.ms/copilot-plugins)
- [Model Context Protocol (MCP) Documentation](https://learn.microsoft.com/en-us/semantic-kernel/model-context-protocol/)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
