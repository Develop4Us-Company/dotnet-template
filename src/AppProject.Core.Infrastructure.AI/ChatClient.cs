using System;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Options;

namespace AppProject.Core.Infrastructure.AI;

public class ChatClient(IOptions<AIOptions> aiOptions)
    : IChatClient
{
    public async Task<string> SendMessageAsync(string systemMessage, IEnumerable<string> userMessages, string model, CancellationToken cancellationToken = default)
    {
        var optionsValue = aiOptions.Value;
        var endpoint = optionsValue.Endpoint;
        var token = optionsValue.Token;

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("AI options are not configured properly.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model must be provided.", nameof(model));
        }

        var credential = new AzureKeyCredential(token);

        var client = new ChatCompletionsClient(
            new Uri(endpoint),
            credential,
            new AzureAIInferenceClientOptions());

        var requestOptions = new ChatCompletionsOptions()
        {
            Model = model,

            // Adjust the temperature, max tokens, and other parameters as needed
        };

        if (!string.IsNullOrEmpty(systemMessage))
        {
            requestOptions.Messages.Add(new ChatRequestSystemMessage(systemMessage));
        }

        foreach (var message in userMessages)
        {
            requestOptions.Messages.Add(new ChatRequestUserMessage(message));
        }

        Response<ChatCompletions> response = await client.CompleteAsync(requestOptions, cancellationToken);
        return response?.Value?.Content ?? string.Empty;
    }
}
