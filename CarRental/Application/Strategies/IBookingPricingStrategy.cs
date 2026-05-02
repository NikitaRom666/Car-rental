using CarRental.Domain;

namespace CarRental.Application.Strategies
{
    public interface IBookingPricingStrategy
    {
        decimal CalculatePrice(Car car, BookingPeriod period);
    }
}