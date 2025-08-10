using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace Chatallotbot.Server.Chat;

public interface IChatCompletionAgentFactory
{
    ChatCompletionAgent CreateAgent();
}

public class ChatCompletionAgentFactory(MsiAuth msiAuth) : IChatCompletionAgentFactory
{
    public ChatCompletionAgent CreateAgent()
    {
        return new ChatCompletionAgent
        {
            Name = "KnowledgeBaseAssistant",
            Instructions = """
                           You are an assistant that ONLY provides information found in the retrieved documents.
                           If the information cannot be found in the retrieved documents, say 'I don't have information about this in my knowledge base.'
                           Do not make up or infer information that isn't explicitly stated in the retrieved documents.
                           Always cite the source of your information when possible.
                           """,
            Kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    deploymentName: AppConfig.ChatConfig.Model,
                    endpoint: AppConfig.ChatConfig.Endpoint,
                    credentials: msiAuth.AzureCredentials)
                .Build()
        };
    }
}