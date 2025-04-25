using System.Globalization;

namespace everave.server.Components.Common
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo EuropeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

        public static string PrintCET(this DateTime dateTime)
        {
            var localizedTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), EuropeTimeZone);
            return localizedTime.ToString("g", new CultureInfo("de-CH"));
        }
    }
}
