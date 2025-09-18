using System;

namespace AppProject.Models;

public class SearchRequest : IRequest
{
    public int? Take { get; set; }

    public string? SearchText { get; set; }
}
