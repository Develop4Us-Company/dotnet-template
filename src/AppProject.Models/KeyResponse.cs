using System;

namespace AppProject.Models;

public class KeyResponse<TIdType> : IResponse
{
    public TIdType Id { get; set; }
}
