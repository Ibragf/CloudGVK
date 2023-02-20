namespace BackendGVK.Services
{
    public interface IDateProvider
    {
        DateTime GetCurrentDate();
        DateTime GetCustomDateTime(int days = 0, int hours = 0, int minutes = 0);
    }

    class DateProvider : IDateProvider
    {
        public DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }

        public DateTime GetCustomDateTime(int days, int hours, int minutes)
        {
            return DateTime.UtcNow.AddDays(days).AddHours(hours).AddMinutes(minutes);
        }
    }
}
