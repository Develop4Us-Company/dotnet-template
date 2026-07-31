using System;
using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AppProject.Core.Infrastructure.AI;

public class ChatClient(IOptions<AIOptions> aiOptions)
    : IChatClient
{
    public async Task<string> SendSingleMessageAsync(string model, string systemMessage, string userMessage, CancellationToken cancellationToken = default)
    {
        var endpoint = aiOptions.Value.Endpoint;
        var token = aiOptions.Value.Token;

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("AI options are not configured properly.");
        }

        var credential = new ApiKeyCredential(token);

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint),
        };

        var client = new OpenAIClient(credential, clientOptions);

        var chatClient = client.GetChatClient(model);

        var chatMessages = new ChatMessage[]
        {
            new SystemChatMessage(systemMessage),
            new UserChatMessage(userMessage)
        };

        var response = await chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);
        return string.Concat(response.Value.Content.Where(x => x.Text != null).Select(x => x.Text));
    }
}
