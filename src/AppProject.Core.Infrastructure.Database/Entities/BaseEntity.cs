using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Core.Infrastructure.Database.Entities;

public abstract class BaseEntity
{
    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedByUserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string CreatedByUserName { get; set; } = default!;

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    [MaxLength(255)]
    public string? UpdatedByUserName { get; set; }

    [Timestamp]
    [Required]
    public byte[] RowVersion { get; set; } = [];
}
