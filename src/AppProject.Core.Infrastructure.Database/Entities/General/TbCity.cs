using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Cities")]
public class TbCity : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid StateId { get; set; }

    [ForeignKey(nameof(StateId))]
    public TbState State { get; set; } = default!;

    public ICollection<TbNeighborhood> Neighborhoods { get; set; } = new List<TbNeighborhood>();
}
