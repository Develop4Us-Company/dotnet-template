using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class SampleDto : IEntity
{
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public byte[]? RowVersion { get; set; }
}