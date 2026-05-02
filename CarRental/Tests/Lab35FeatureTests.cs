using System;
using System.Collections.Generic;
using System.Linq;
using CarRental.Application;
using CarRental.Application.Dto;
using CarRental.Application.Strategies;
using CarRental.Domain;
using Moq;
using Xunit;

namespace CarRental.Tests
{
    public class Lab35FeatureTests
    {
        [Fact]
        public void Car_EmptyModel_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Car(Guid.NewGuid(), "", VehicleCategory.Economy));
        }

        [Fact]
        public void Customer_EmptyName_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Customer(Guid.NewGuid(), "   "));
        }

        [Fact]
        public void BookingPeriod_InvalidRange_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 10)));
        }

        [Fact]
        public void BookingPeriod_Overlaps_ReturnsTrueForIntersectingPeriods()
        {
            var first = new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12));
            var second = new BookingPeriod(new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 13));

            Assert.True(first.Overlaps(second));
        }

        [Fact]
        public void PricingStrategyFactory_ReturnsCorrectStrategies()
        {
            var factory = new BookingPricingStrategyFactory();
            var period = new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12));
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Standard);

            Assert.Equal(180m, factory.GetStrategy(VehicleCategory.Economy).CalculatePrice(car, period));
            Assert.Equal(240m, factory.GetStrategy(VehicleCategory.Standard).CalculatePrice(car, period));
            Assert.Equal(340m, factory.GetStrategy(VehicleCategory.SUV).CalculatePrice(car, period));
        }

        [Fact]
        public void QueryService_ReturnsActiveBookingsSortedByStartDate()
        {
            var car = new Car(Guid.NewGuid(), "Toyota Yaris", VehicleCategory.Economy);
            var bookings = new List<Booking>
            {
                new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Олена"), new BookingPeriod(new DateOnly(2026, 5, 14), new DateOnly(2026, 5, 16)), 180m),
                new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Іван"), new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)), 180m)
            };

            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetAll()).Returns(new[] { car });
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(bookings);

            var service = new BookingQueryService(carRepo.Object, bookingRepo.Object);
            var result = service.GetActiveBookings();

            Assert.Equal(2, result.Count);
            Assert.Equal("Іван", result.First().CustomerName);
        }

        [Fact]
        public void QueryService_PopularityReport_UsesBookingCounts()
        {
            var car1 = new Car(Guid.NewGuid(), "Toyota", VehicleCategory.Economy);
            var car2 = new Car(Guid.NewGuid(), "BMW", VehicleCategory.SUV);
            var customer = new Customer(Guid.NewGuid(), "Іван");
            var bookings = new List<Booking>
            {
                new Booking(Guid.NewGuid(), car1, customer, new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)), 180m),
                new Booking(Guid.NewGuid(), car1, customer, new BookingPeriod(new DateOnly(2026, 5, 13), new DateOnly(2026, 5, 15)), 180m),
                new Booking(Guid.NewGuid(), car2, customer, new BookingPeriod(new DateOnly(2026, 5, 16), new DateOnly(2026, 5, 18)), 340m)
            };

            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetAll()).Returns(new[] { car1, car2 });
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(bookings);

            var service = new BookingQueryService(carRepo.Object, bookingRepo.Object);
            var result = service.GetMostPopularCars();

            Assert.Equal(2, result.First().BookingCount);
            Assert.Equal("Toyota", result.First().Model);
        }

        [Fact]
        public void QueryService_RevenueByCategory_GroupsTotals()
        {
            var car1 = new Car(Guid.NewGuid(), "Toyota", VehicleCategory.Economy);
            var car2 = new Car(Guid.NewGuid(), "BMW", VehicleCategory.SUV);
            var customer = new Customer(Guid.NewGuid(), "Іван");
            var bookings = new List<Booking>
            {
                new Booking(Guid.NewGuid(), car1, customer, new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)), 180m),
                new Booking(Guid.NewGuid(), car2, customer, new BookingPeriod(new DateOnly(2026, 5, 16), new DateOnly(2026, 5, 18)), 340m)
            };

            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetAll()).Returns(new[] { car1, car2 });
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(bookings);

            var service = new BookingQueryService(carRepo.Object, bookingRepo.Object);
            var result = service.GetRevenueByCategory();

            Assert.Equal(180m, result[VehicleCategory.Economy]);
            Assert.Equal(340m, result[VehicleCategory.SUV]);
        }
    }
}
