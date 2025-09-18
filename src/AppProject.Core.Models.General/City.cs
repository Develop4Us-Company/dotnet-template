using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Core.Models.CustomValidators;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class City : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid StateId { get; set; }

    public byte[]? RowVersion { get; set; }

    [ValidateCollection]
    public IList<CreateOrUpdateRequest<Neighborhood>> ChangedNeighborhoodRequests { get; set; } = new List<CreateOrUpdateRequest<Neighborhood>>();

    [ValidateCollection]
    public IList<DeleteRequest<Guid>> DeletedNeighborhoodRequests { get; set; } = new List<DeleteRequest<Guid>>();
}
