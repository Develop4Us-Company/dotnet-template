using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("States")]
public class TbState : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    required public string Name { get; set; }

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    required public TbCountry Country { get; set; }

    public ICollection<TbCity> Cities { get; set; } = new List<TbCity>();
}
