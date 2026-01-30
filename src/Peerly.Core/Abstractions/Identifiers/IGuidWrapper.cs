using System;

namespace Peerly.Core.Abstractions.Identifiers;

public interface IGuidWrapper<T>
    where T : struct, IGuidWrapper<T>
{
    static abstract explicit operator T(Guid value);
    static abstract explicit operator Guid(T wrapper);
}
