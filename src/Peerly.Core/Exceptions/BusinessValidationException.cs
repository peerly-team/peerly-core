using System;
using System.Diagnostics.CodeAnalysis;

namespace Peerly.Core.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class BusinessValidationException : Exception
{
    public BusinessValidationException(string message)
        : base(message)
    {
    }
}

