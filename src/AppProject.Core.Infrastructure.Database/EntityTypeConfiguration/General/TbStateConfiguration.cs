using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbStateConfiguration : IEntityTypeConfiguration<TbState>
{
    public void Configure(EntityTypeBuilder<TbState> builder)
    {
        builder.HasIndex(x => x.Name);
    }
}
