using System;
using System.ComponentModel.DataAnnotations;

namespace AppProject.Models.CustomValidators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RequiredGuidAttribute : RequiredAttribute
{
    public RequiredGuidAttribute()
    {
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return false;
        }

        Guid guidValue;

        if (value is Guid guid)
        {
            guidValue = guid;
        }
        else if (value != null && value.GetType() == typeof(Guid?))
        {
            var nullableGuid = (Guid?)value;
            if (!nullableGuid.HasValue)
            {
                return false;
            }

            guidValue = nullableGuid.Value;
        }
        else if (value is string stringValue && Guid.TryParse(stringValue, out var parsedGuid))
        {
            guidValue = parsedGuid;
        }
        else
        {
            return base.IsValid(value);
        }

        return guidValue == Guid.Empty
            ? false
            : true;
    }
}
