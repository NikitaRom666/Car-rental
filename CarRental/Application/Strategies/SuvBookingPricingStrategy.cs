using CarRental.Domain;

namespace CarRental.Application.Strategies
{
    public sealed class SuvBookingPricingStrategy : IBookingPricingStrategy
    {
        public decimal CalculatePrice(Car car, BookingPeriod period)
        {
            return period.DurationDays * 170m;
        }
    }
}