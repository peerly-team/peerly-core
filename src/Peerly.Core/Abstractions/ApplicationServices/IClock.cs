using System;

namespace Peerly.Core.Abstractions.ApplicationServices;

public interface IClock
{
    DateTimeOffset GetCurrentMoscowDateTime();
    DateTimeOffset GetCurrentTime();
}
