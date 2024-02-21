using System;
using System.Globalization;

public static class DateUtilities
{
    private const string DateFormat = "yyyy-MM-ddTHH:mm:ss";

    public static string Convert(DateTime _date)
    {
        return _date.ToString(DateFormat);
    }

    public static DateTime Convert(string _dateString)
    {
        return DateTime.ParseExact(_dateString, DateFormat, CultureInfo.InvariantCulture);
    }
}