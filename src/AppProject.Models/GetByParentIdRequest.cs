using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models.CustomValidators;

namespace AppProject.Models;

public class GetByParentIdRequest<TIdType> : IRequest
{
    [RequiredGuid]
    required public TIdType ParentId { get; set; }
}
