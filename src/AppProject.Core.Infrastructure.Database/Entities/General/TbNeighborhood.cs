using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Neighborhoods")]
public class TbNeighborhood : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public TbCity City { get; set; } = default!;
}
