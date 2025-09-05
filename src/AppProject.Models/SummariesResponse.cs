using System;

namespace AppProject.Models;

public class SummariesResponse<TSummary> : IResponse
    where TSummary : class, ISummary
{
    required public IReadOnlyCollection<TSummary> Summaries { get; set; }
}