using System;
using Peerly.Core.Abstractions.Identifiers;

namespace Peerly.Core.Identifiers;

public readonly record struct TemporaryStorageId : IGuidWrapper<TemporaryStorageId>
{
    private readonly Guid _value;

    private TemporaryStorageId(Guid value)
    {
        _value = value;
    }

    public static explicit operator TemporaryStorageId(Guid value) => new(value);
    public static explicit operator Guid(TemporaryStorageId temporaryStorageId) => temporaryStorageId._value;

    public override string ToString()
    {
        return _value.ToString();
    }
}
