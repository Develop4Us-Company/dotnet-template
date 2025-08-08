using System;

namespace AppProject.Models;

public interface IEntity
{
    public byte[]? RowVersion { get; set; }
}
