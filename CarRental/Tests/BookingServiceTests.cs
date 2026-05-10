using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Application;
using CarRental.Application.Dto;
using CarRental.Domain;
using Moq;
using Xunit;

namespace CarRental.Tests
{
    public class BookingServiceTests
    {
        [Fact]
        public void CreateBooking_Success()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            carRepo.Setup(r => r.GetAll()).Returns(new List<Car> { car });
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today.AddDays(3)
            };

            var result = service.CreateBooking(request);

            Assert.True(result.Success);
            Assert.Contains("успішне", result.Message);
            bookingRepo.Verify(r => r.Add(It.IsAny<Booking>()), Times.Once);
            carRepo.Verify(r => r.Update(It.IsAny<Car>()), Times.Once);
        }

        [Fact]
        public void CreateBooking_UnavailableCar_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Booked);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today.AddDays(3)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("недоступне", result.Message);
        }

        [Theory]
        [InlineData("2025-12-31", "2026-01-01")]
        [InlineData("2026-05-10", "2026-05-10")]
        [InlineData("2026-05-10", "2026-05-09")]
        public async Task CreateBooking_InvalidDateRange_ReturnsError(string start, string end)
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Parse(start),
                End = DateTime.Parse(end)
            };

            var result = await service.CreateBookingAsync(request);

            Assert.False(result.Success);
        }

        [Fact]
        public void CreateBooking_Overlap_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            var existingBooking = new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Test"), period, 100);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking> { existingBooking });
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(4)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("заброньоване", result.Message);
        }

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
                CustomerName = "Іван Петренко",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("ID авто не задано", result.Message);
        }

        [Fact]
        public void CreateBooking_EmptyCustomerId_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.Empty,
                CustomerName = "Іван Петренко",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("ID клієнта не задано", result.Message);
        }

        [Fact]
        public void CreateBooking_EmptyCustomerName_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("Ім'я клієнта", result.Message);
        }

        [Fact]
        public async Task CancelBooking_Success_ReleasesCar()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Booked);
            var booking = new Booking(
                Guid.NewGuid(),
                car,
                new Customer(Guid.NewGuid(), "Іван Петренко"),
                new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(2)), DateOnly.FromDateTime(DateTime.Today.AddDays(4))),
                180m);

            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetById(booking.Id)).Returns(booking);
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var result = await service.CancelBookingAsync(booking.Id);

            Assert.True(result.Success);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(AvailabilityStatus.Available, car.AvailabilityStatus);
            bookingRepo.Verify(r => r.Update(It.IsAny<Booking>()), Times.Once);
            carRepo.Verify(r => r.Update(It.IsAny<Car>()), Times.Once);
        }

        [Fact]
        public void CreateBooking_CarNotFound_ReturnsError()
        {
            var carId = Guid.NewGuid();
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(carId)).Returns((Car?)null);
            var bookingRepo = new Mock<IBookingRepository>();
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = carId,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today.AddDays(3)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("не знайдено", result.Message);
        }

        [Fact]
        public void CreateBooking_CustomerMaxActiveBookings_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Available);
            var customerId = Guid.NewGuid();
            var period1 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(10)), DateOnly.FromDateTime(DateTime.Today.AddDays(12)));
            var period2 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(13)), DateOnly.FromDateTime(DateTime.Today.AddDays(15)));
            var period3 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(16)), DateOnly.FromDateTime(DateTime.Today.AddDays(18)));
            
            var activeBookings = new List<Booking>
            {
                new Booking(Guid.NewGuid(), car, new Customer(customerId, "Test"), period1, 100m, BookingStatus.Active),
                new Booking(Guid.NewGuid(), car, new Customer(customerId, "Test"), period2, 100m, BookingStatus.Active),
                new Booking(Guid.NewGuid(), car, new Customer(customerId, "Test"), period3, 100m, BookingStatus.Active)
            };

            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetById(car.Id)).Returns(car);
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(activeBookings);
            bookingRepo.Setup(r => r.GetOverlappingAsync(car.Id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<Booking>());
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = customerId,
                CustomerName = "Test",
                Start = DateTime.Today.AddDays(19),
                End = DateTime.Today.AddDays(21)
            };

            var result = service.CreateBooking(request);

            Assert.False(result.Success);
            Assert.Contains("максимальна", result.Message.ToLower());
        }

        [Fact]
        public async Task CancelBooking_AlreadyStarted_ReturnsError()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Booked);
            var booking = new Booking(
                Guid.NewGuid(),
                car,
                new Customer(Guid.NewGuid(), "Іван Петренко"),
                new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateOnly.FromDateTime(DateTime.Today.AddDays(2))),
                180m);

            var carRepo = new Mock<ICarRepository>();
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetById(booking.Id)).Returns(booking);
            var service = new BookingService(carRepo.Object, bookingRepo.Object);

            var result = await service.CancelBookingAsync(booking.Id);

            Assert.False(result.Success);
            Assert.Contains("почалося", result.Message);
        }
    }
}
