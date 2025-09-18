using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class State : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    public byte[]? RowVersion { get; set; }
}
