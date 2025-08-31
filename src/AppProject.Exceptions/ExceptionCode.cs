﻿namespace AppProject.Exceptions;

public enum ExceptionCode
{
    Generic,
    SecurityValidation,
    RequestValidation,
    Concurrency,
    EntityNotFound,

    // Pattern: ModuleName_EntityName_ValidationName
    General_Country_DuplicateName,
    General_State_DuplicateName,
}
