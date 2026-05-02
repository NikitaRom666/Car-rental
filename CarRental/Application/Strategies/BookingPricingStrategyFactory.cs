using System.Collections.Generic;
using CarRental.Domain;

namespace CarRental.Application.Strategies
{
    public sealed class BookingPricingStrategyFactory
    {
        private readonly Dictionary<VehicleCategory, IBookingPricingStrategy> _strategies;

        public BookingPricingStrategyFactory()
        {
            _strategies = new Dictionary<VehicleCategory, IBookingPricingStrategy>
            {
                [VehicleCategory.Economy] = new EconomyBookingPricingStrategy(),
                [VehicleCategory.Standard] = new StandardBookingPricingStrategy(),
                [VehicleCategory.SUV] = new SuvBookingPricingStrategy()
            };
        }

        public IBookingPricingStrategy GetStrategy(VehicleCategory category)
        {
            return _strategies[category];
        }
    }
}