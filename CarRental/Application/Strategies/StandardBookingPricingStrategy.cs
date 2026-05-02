using CarRental.Domain;

namespace CarRental.Application.Strategies
{
    public sealed class StandardBookingPricingStrategy : IBookingPricingStrategy
    {
        public decimal CalculatePrice(Car car, BookingPeriod period)
        {
            return period.DurationDays * 120m;
        }
    }
}