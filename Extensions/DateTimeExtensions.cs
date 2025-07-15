using System.Globalization;

namespace PrimeGames.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly CultureInfo ThaiCulture = new CultureInfo("th-TH");

        public static string ToThaiDateString(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy", ThaiCulture);
        }

        public static string ToThaiDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy HH:mm", ThaiCulture);
        }

        public static string ToRelativeTime(this DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return "เมื่อสักครู่";

            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{timeSpan.Minutes} นาทีที่แล้ว";

            if (timeSpan <= TimeSpan.FromHours(24))
                return $"{timeSpan.Hours} ชั่วโมงที่แล้ว";

            if (timeSpan <= TimeSpan.FromDays(7))
                return $"{timeSpan.Days} วันที่แล้ว";

            if (timeSpan <= TimeSpan.FromDays(30))
            {
                int weeks = (int)(timeSpan.Days / 7);
                return $"{weeks} สัปดาห์ที่แล้ว";
            }

            if (timeSpan <= TimeSpan.FromDays(365))
            {
                int months = (int)(timeSpan.Days / 30);
                return $"{months} เดือนที่แล้ว";
            }

            int years = (int)(timeSpan.Days / 365);
            return $"{years} ปีที่แล้ว";
        }
    }
}