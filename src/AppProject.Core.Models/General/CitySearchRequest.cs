using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySearchRequest : SearchRequest
{
    public Guid? StateId { get; set; }
}
