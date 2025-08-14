using System;

namespace AppProject.Core.Infrastructure.AI;

public interface IChatClient
{
    Task<string> SendMessageAsync(
        string systemMessage,
        IEnumerable<string> userMessages,
        string model,
        CancellationToken cancellationToken = default);
}
