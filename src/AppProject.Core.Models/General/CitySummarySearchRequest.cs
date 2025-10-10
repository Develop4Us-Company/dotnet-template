using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySummarySearchRequest : SearchRequest
{
    public Guid? StateId { get; set; }
}
