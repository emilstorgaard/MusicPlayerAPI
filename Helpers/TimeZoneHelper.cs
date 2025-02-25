using System;

namespace MusicPlayerAPI.Helpers;

public static class TimeZoneHelper
{
    private static TimeZoneInfo _timeZoneInfo;

    static TimeZoneHelper()
    {
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
    }

    public static DateTime GetCopenhagenTime(DateTime utcDateTime)
    {
        if (_timeZoneInfo == null) throw new InvalidOperationException("TimeZoneInfo has not been initialized.");

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZoneInfo);
    }
}
