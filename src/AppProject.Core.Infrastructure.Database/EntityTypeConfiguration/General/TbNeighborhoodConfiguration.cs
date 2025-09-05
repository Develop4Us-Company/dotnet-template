using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbNeighborhoodConfiguration : IEntityTypeConfiguration<TbNeighborhood>
{
    public void Configure(EntityTypeBuilder<TbNeighborhood> builder)
    {
        builder.HasIndex(x => x.Name);
    }
}
