using System;

namespace AppProject.Models;

public class KeyResponse<TIdType> : IResponse
{
    public required TIdType Id { get; set; }
}
