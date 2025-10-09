using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSearchRequest : SearchRequest
{
    public Guid? CountryId { get; set; }
}
