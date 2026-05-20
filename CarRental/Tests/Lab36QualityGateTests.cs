using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CarRental.Application;
using CarRental.Application.Dto;
using CarRental.Domain;
using CarRental.Infrastructure;
using Moq;
using Xunit;

namespace CarRental.Tests
{
    public class Lab36QualityGateTests
    {
        [Fact]
        public void Booking_CannotBeCancelledTwice()
        {
            var car = new Car(Guid.NewGuid(), "Toyota", VehicleCategory.Economy);
            var booking = new Booking(
                Guid.NewGuid(),
                car,
                new Customer(Guid.NewGuid(), "Іван"),
                new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)),
                180m);

            booking.Cancel();

            Assert.Throws<InvalidOperationException>(() => booking.Cancel());
        }

        [Fact]
        public void Car_UnavailableCannotBeOpened()
        {
            var car = new Car(Guid.NewGuid(), "Toyota", VehicleCategory.Economy, AvailabilityStatus.Unavailable);

            Assert.Throws<InvalidOperationException>(() => car.MarkAvailable());
        }

        [Fact]
        public void BookingPeriod_Overlaps_Null_Throws()
        {
            var first = new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12));

            Assert.Throws<ArgumentNullException>(() => first.Overlaps(null!));
        }

        [Fact]
        public void QueryService_Search_ByCarModel_ReturnsMatches()
        {
            var car = new Car(Guid.NewGuid(), "BMW X5", VehicleCategory.SUV);
            var booking = new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Олена"), new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)), 340m);
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetAll()).Returns(new[] { car });
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(new[] { booking });

            var service = new BookingQueryService(carRepo.Object, bookingRepo.Object);
            var result = service.SearchBookings("BMW");

            Assert.Single(result);
            Assert.Equal("BMW X5", result[0].CarModel);
        }

        [Fact]
        public void QueryService_Search_EmptyInput_ReturnsEmptyList()
        {
            var carRepo = new Mock<ICarRepository>();
            carRepo.Setup(r => r.GetAll()).Returns(Array.Empty<Car>());
            var bookingRepo = new Mock<IBookingRepository>();
            bookingRepo.Setup(r => r.GetAll()).Returns(Array.Empty<Booking>());

            var service = new BookingQueryService(carRepo.Object, bookingRepo.Object);
            var result = service.SearchBookings(string.Empty);

            Assert.Empty(result);
        }

        [Fact]
        public void CarRepository_CorruptedJson_UsesDefaultCars()
        {
            var tempDir = CreateTempDirectory();
            var filePath = Path.Combine(tempDir, "cars.json");
            File.WriteAllText(filePath, "{ broken json");

            Assert.Throws<PersistenceException>(() => new FileCarRepository(filePath));
        }

        [Fact]
        public void BookingRepository_MissingFile_StartsEmpty()
        {
            var tempDir = CreateTempDirectory();
            var filePath = Path.Combine(tempDir, "missing-bookings.json");

            var repository = new FileBookingRepository(filePath);

            Assert.Empty(repository.GetAll());
        }

        [Fact]
        public void BookingRepository_CorruptedJson_StartsEmpty()
        {
            var tempDir = CreateTempDirectory();
            var filePath = Path.Combine(tempDir, "bookings.json");
            File.WriteAllText(filePath, "not-json");

            Assert.Throws<PersistenceException>(() => new FileBookingRepository(filePath));
        }

        [Fact]
        public async Task CancelBooking_PersistsAfterReload()
        {
            var tempDir = CreateTempDirectory();
            var carsPath = Path.Combine(tempDir, "cars.json");
            var bookingsPath = Path.Combine(tempDir, "bookings.json");

            var carRepository = new FileCarRepository(carsPath);
            var bookingRepository = new FileBookingRepository(bookingsPath);
            var service = new BookingService(carRepository, bookingRepository);
            var car = carRepository.GetAll().First();

            var createResult = service.CreateBooking(new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Іван Петренко",
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(4)
            });

            Assert.True(createResult.Success);

            var cancelResult = await service.CancelBookingAsync(createResult.BookingId!.Value);
            Assert.True(cancelResult.Success);

            var reloadedCars = new FileCarRepository(carsPath);
            var reloadedBookings = new FileBookingRepository(bookingsPath);

            Assert.Contains(reloadedCars.GetAll(), item => item.Id == car.Id && item.AvailabilityStatus == AvailabilityStatus.Available);
            Assert.Contains(reloadedBookings.GetAll(), item => item.Id == createResult.BookingId && item.Status == BookingStatus.Cancelled);
        }

        [Fact]
        public void TwoSequentialBookings_ArePersisted()
        {
            var tempDir = CreateTempDirectory();
            var carsPath = Path.Combine(tempDir, "cars.json");
            var bookingsPath = Path.Combine(tempDir, "bookings.json");

            var carRepository = new FileCarRepository(carsPath);
            var bookingRepository = new FileBookingRepository(bookingsPath);
            var service = new BookingService(carRepository, bookingRepository);
            var cars = carRepository.GetAll().Take(2).ToList();

            var first = service.CreateBooking(new BookingRequestDto
            {
                CarId = cars[0].Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Олена",
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(4)
            });

            var second = service.CreateBooking(new BookingRequestDto
            {
                CarId = cars[1].Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Ірина",
                Start = DateTime.Today.AddDays(2),
                End = DateTime.Today.AddDays(4)
            });

            Assert.True(first.Success);
            Assert.True(second.Success);

            var reloadedBookings = new FileBookingRepository(bookingsPath);
            Assert.Equal(2, reloadedBookings.GetAll().Count());
        }

        private static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "CarRentalTestsLab36", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
