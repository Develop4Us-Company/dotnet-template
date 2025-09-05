using System;

namespace AppProject.Models;

public class KeyResponse<TIdType> : IResponse
{
    required public TIdType Id { get; set; }
}
