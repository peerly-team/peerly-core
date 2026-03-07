using System;
using Peerly.Core.Abstractions.ApplicationServices;

namespace Peerly.Core.ApplicationServices.Providers;

internal sealed class Clock : IClock
{
    public DateTimeOffset GetCurrentMoscowDateTime()
    {
        var russianStandardTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.Now,
            TimeZoneInfo.Local.Id,
            "Russian Standard Time");
        return new DateTimeOffset(russianStandardTime);
    }

    public DateTimeOffset GetCurrentTime()
    {
        return DateTimeOffset.UtcNow;
    }
}
