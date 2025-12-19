using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class SampleDto : IEntity
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    public byte[]? RowVersion { get; set; }
}
