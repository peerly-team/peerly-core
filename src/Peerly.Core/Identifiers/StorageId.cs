using System;
using Peerly.Core.Abstractions.Identifiers;

namespace Peerly.Core.Identifiers;

public readonly record struct StorageId : IGuidWrapper<StorageId>
{
    private readonly Guid _value;

    private StorageId(Guid value)
    {
        _value = value;
    }

    public static explicit operator StorageId(Guid value) => new(value);
    public static explicit operator Guid(StorageId storageId) => storageId._value;

    public override string ToString()
    {
        return _value.ToString();
    }
}
