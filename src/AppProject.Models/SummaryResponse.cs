using System;

namespace AppProject.Models;

public class SummaryResponse<TSummary> : IResponse
    where TSummary : class, ISummary
{
    public required TSummary Summary { get; set; }
}
