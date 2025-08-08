using System;

namespace AppProject.Models;

public class SummaryResponse<TSummary> : IResponse
    where TSummary : class, ISummary
{
    public TSummary Summary { get; set; } = default!;
}
