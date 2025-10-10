using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSummarySearchRequest : SearchRequest
{
    public Guid? CountryId { get; set; }
}
