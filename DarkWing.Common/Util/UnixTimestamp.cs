using System;

namespace DarkWing.Common.Util;

public static class UnixTimestamp
{
    public static long ToUnixTs(this DateTime dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
    }

    public static DateTime ToDateTime(this long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(timestamp);
    }
    
    public static long ToUnixTs(this DateTimeOffset dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
    }

    public static long AddDays(this long timestamp, int days) =>
        (timestamp + days * 24 * 60 * 60 * 1000);

    public static long AddMinutes(this long timestamp, int minutes) =>
        (timestamp + minutes * 60 * 1000);

    public static (long, long) GetStartAndEndOfTheDay(this long unxTime)
    {
        var dt = unxTime.ToDateTime();
        var start = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
        var end = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
        return (start.ToUnixTs(), end.ToUnixTs());
    }

    public static long GetDiffInMinutes(this long first, long second) =>
        ((second + 1000) - first) / 60;
}
