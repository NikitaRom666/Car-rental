using CarRental.Domain;

namespace CarRental.Application.Strategies
{
    public sealed class EconomyBookingPricingStrategy : IBookingPricingStrategy
    {
        public decimal CalculatePrice(Car car, BookingPeriod period)
        {
            return period.DurationDays * 90m;
        }
    }
}