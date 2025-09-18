using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Countries")]
public class TbCountry : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    public ICollection<TbState> States { get; set; } = new List<TbState>();
}
