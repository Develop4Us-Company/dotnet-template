using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbCityConfiguration : IEntityTypeConfiguration<TbCity>
{
    public void Configure(EntityTypeBuilder<TbCity> builder)
    {
        builder.HasIndex(x => x.Name);
    }
}
