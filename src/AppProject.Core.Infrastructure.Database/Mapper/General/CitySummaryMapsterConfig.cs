using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.General;

public class CitySummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TbCity, CitySummary>()
            .Map(dest => dest.StateName, src => src.State.Name)
            .Map(dest => dest.CountryName, src => src.State.Country.Name)
            .Map(dest => dest.CountryId, src => src.State.CountryId);
    }
}
