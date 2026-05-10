using System;

namespace CarRental.Domain
{
    public class BookingPeriod
    {
        public DateOnly Start { get; }
        public DateOnly End { get; }
        public int DurationDays => End.DayNumber - Start.DayNumber;

        public BookingPeriod(DateOnly start, DateOnly end)
        {
            if (start >= end)
                throw new ArgumentException("Некоректний період: дата завершення має бути після початку");
            if ((end.DayNumber - start.DayNumber) < 1)
                throw new ArgumentException("Мінімальний період бронювання — 1 день");
            Start = start;
            End = end;
        }

        public bool Overlaps(BookingPeriod other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Start < other.End && End > other.Start;
        }
    }
}
