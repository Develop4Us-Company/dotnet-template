using System;

namespace AppProject.Core.Infrastructure.AI;

public interface IChatClient
{
    Task<string> SendSingleMessageAsync(
        string model,
        string systemMessage,
        string userMessage,
        CancellationToken cancellationToken = default);
}
