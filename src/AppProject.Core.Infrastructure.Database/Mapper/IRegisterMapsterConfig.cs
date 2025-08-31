using System;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper;

public interface IRegisterMapsterConfig
{
    void Register(TypeAdapterConfig config);
}
