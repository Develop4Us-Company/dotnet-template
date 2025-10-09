using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;
using AppProject.Models.CustomValidators;

namespace AppProject.Core.Models.General;

public class City : IEntity
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [RequiredGuid]
    public Guid StateId { get; set; }

    public byte[]? RowVersion { get; set; }

    [ValidateCollection]
    public IList<CreateOrUpdateRequest<Neighborhood>> ChangedNeighborhoodRequests { get; set; } = new List<CreateOrUpdateRequest<Neighborhood>>();

    [ValidateCollection]
    public IList<DeleteRequest<Guid>> DeletedNeighborhoodRequests { get; set; } = new List<DeleteRequest<Guid>>();
}
