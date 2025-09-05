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
    required public string Name { get; set; }

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid StateId { get; set; }

    [ForeignKey(nameof(StateId))]
    required public TbState State { get; set; }

    public ICollection<TbNeighborhood> Neighborhoods { get; set; } = new List<TbNeighborhood>();
}
