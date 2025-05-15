# Azure AI Chat Agent

## Overview
This project implements an intelligent chat application built with Azure OpenAI services, SignalR, and Semantic Kernel. The application provides a conversational interface that can understand user intents, process natural language requests, and interact with various services to provide responses.

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

### Azure Services
- Azure OpenAI Service
- Azure Identity for authentication

## Architecture

The application follows a service-oriented architecture with the following key components:

### Core Components

1. **ChatHub (SignalR)**: Manages real-time communication between clients and server
2. **ChatKernelPlugin**: Integrates with Semantic Kernel to provide AI capabilities
3. **AzureOpenAIService**: Handles communication with Azure OpenAI
4. **BigCatService**: Get recommended products for the user
5. **CaseService**: Create customer account in CASE portal.

### AI Features

- Intent recognition for understanding user requests
- Product recommendation capabilities
- Customer account creation flow
- Natural language processing for various use cases

## Setup and Configuration

### Prerequisites
- .NET 8.0 SDK
- Azure account with Azure OpenAI service configured
- Visual Studio 2022 or later (recommended)
- SignalR Client Library
- Nugets
- ![image](https://github.com/user-attachments/assets/ff2bd92b-3238-49d6-aa3a-32f3444cea69)


### Environment Configuration
Configure the following in your appsettings.json:

```json
{
  "AzureOpenAI": {
    "Endpoint": "your-azure-openai-endpoint",
    "Key": "your-azure-openai-key",
    "DeploymentName": "your-deployment-name",
    "ModelId": "your-model-id"
  }
}
