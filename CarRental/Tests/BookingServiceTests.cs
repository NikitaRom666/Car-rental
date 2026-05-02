using System;
using Xunit;
using CarRental.Domain;
using CarRental.Application;
using CarRental.Application.Dto;

using CarRental.Infrastructure;
using System.Linq;
using Moq;

namespace CarRental.Tests
{
    // Тести для BookingService
    public class BookingServiceTests
    {
        // Успішне бронювання
        [Fact]
        public void CreateBooking_Success()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetByCar(car.Id)).Returns(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(2)
            };

            var result = service.CreateBooking(request);
            Assert.True(result.Success);
            Assert.Contains("успішне", result.Message);
        }

        // Недоступне авто
        [Fact]
        public void CreateBooking_UnavailableCar_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Booked);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetByCar(car.Id)).Returns(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(2)
            };

            var result = service.CreateBooking(request);
            Assert.False(result.Success);
            Assert.Contains("недоступне", result.Message);
        }


        // Некоректні діапазони дат (async)
        [Theory]
        [InlineData("2025-12-31", "2026-01-01")]  // fail - минуле
        [InlineData("2026-05-10", "2026-05-10")]  // fail - той самий день
        [InlineData("2026-05-10", "2026-05-09")]  // fail - кінець раніше початку
        public async System.Threading.Tasks.Task CreateBooking_InvalidDateRange_ReturnsError(string start, string end)
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = DateTime.Parse(start),
                End = DateTime.Parse(end)
            };

            var result = await service.CreateBookingAsync(request);
            Assert.False(result.Success);
        }

        // Перетин з існуючим бронюванням
        [Fact]
        public void CreateBooking_Overlap_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
            var existingBooking = new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Test"), period, 100);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetByCar(car.Id)).Returns(new List<Booking> { existingBooking });
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking> { existingBooking });
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today.AddDays(3)
            };

            var result = service.CreateBooking(request);
            Assert.False(result.Success);
            Assert.Contains("заброньоване", result.Message);
        }

        // Null request
        [Fact]
        public void CreateBooking_NullRequest_ReturnsError()
        {
            var carRepo = new Mock<ICarRepository>();
            var bookingRepo = new Mock<IBookingRepository>();
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var result = service.CreateBooking(null);
            Assert.False(result.Success);
            Assert.Contains("порожнім", result.Message);
        }

        // Порожній CarId
        [Fact]
        public void CreateBooking_EmptyCarId_ReturnsError()
        {
            var carRepo = new Mock<ICarRepository>();
            var bookingRepo = new Mock<IBookingRepository>();
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = Guid.Empty,
                CustomerId = Guid.NewGuid(),
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };

            var result = service.CreateBooking(request);
            Assert.False(result.Success);
            Assert.Contains("ID авто не задано", result.Message);
        }

        // Порожній CustomerId
        [Fact]
        public void CreateBooking_EmptyCustomerId_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetByCar(car.Id)).Returns(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.Empty,
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };

            var result = service.CreateBooking(request);
            Assert.False(result.Success);
            Assert.Contains("ID клієнта не задано", result.Message);
        }

        // Порожні дати
        [Fact]
        public void CreateBooking_EmptyDates_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetByCar(car.Id)).Returns(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = default,
                End = default
            };

            var result = service.CreateBooking(request);
            Assert.False(result.Success);
            Assert.Contains("Дати не можуть", result.Message);
        }
    }
}
