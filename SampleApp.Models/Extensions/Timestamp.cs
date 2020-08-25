using System;

namespace SampleApp.Data.Extensions
{
    public static class Timestamp
    {
        public static DateTime Local(string timeZoneId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            }
            catch 
            {
                return DateTime.UtcNow;
            }
        }
    }
}
