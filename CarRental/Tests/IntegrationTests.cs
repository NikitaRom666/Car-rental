using System;
using System.IO;
using System.Linq;
using CarRental.Application;
using CarRental.Application.Dto;
using CarRental.Domain;
using CarRental.Infrastructure;
using Xunit;

namespace CarRental.Tests
{
    // Інтеграційні тести для файлів і use case
    public class IntegrationTests
    {
        [Fact]
        public void CarRepository_SaveAndReload_PreservesCars()
        {
            var tempDir = CreateTempDirectory();
            var carsPath = Path.Combine(tempDir, "cars.json");

            var firstRepository = new FileCarRepository(carsPath);
            var firstCars = firstRepository.GetAll().ToList();

            var secondRepository = new FileCarRepository(carsPath);
            var secondCars = secondRepository.GetAll().ToList();

            Assert.Equal(3, firstCars.Count);
            Assert.Equal(3, secondCars.Count);
            Assert.Equal(firstCars.Select(c => c.Model), secondCars.Select(c => c.Model));
        }

        [Fact]
        public void BookingRepository_AddAndReload_PreservesBooking()
        {
            var tempDir = CreateTempDirectory();
            var bookingsPath = Path.Combine(tempDir, "bookings.json");
            var car = new Car(Guid.NewGuid(), "Test Car", VehicleCategory.Economy, AvailabilityStatus.Available);
            var booking = new Booking(
                Guid.NewGuid(),
                car,
                new Customer(Guid.NewGuid(), "Test Customer"),
                new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)),
                200m);

            var firstRepository = new FileBookingRepository(bookingsPath);
            firstRepository.Add(booking);

            var secondRepository = new FileBookingRepository(bookingsPath);
            var loadedBooking = secondRepository.GetAll().FirstOrDefault();

            Assert.NotNull(loadedBooking);
            Assert.Equal(booking.Id, loadedBooking!.Id);
            Assert.Equal(booking.Car.Model, loadedBooking.Car.Model);
        }

        [Fact]
        public void BookingService_WithRealRepositories_PersistsBookingAndUpdatesCar()
        {
            var tempDir = CreateTempDirectory();
            var carsPath = Path.Combine(tempDir, "cars.json");
            var bookingsPath = Path.Combine(tempDir, "bookings.json");

            var carRepository = new FileCarRepository(carsPath);
            var bookingRepository = new FileBookingRepository(bookingsPath);
            var service = new BookingService(carRepository, bookingRepository);
            var car = carRepository.GetAll().First();

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                Start = new DateTime(2026, 5, 10),
                End = new DateTime(2026, 5, 12)
            };

            var result = service.CreateBooking(request);

            Assert.True(result.Success);
            Assert.NotNull(result.BookingId);

            var reloadedCars = new FileCarRepository(carsPath);
            var reloadedBookings = new FileBookingRepository(bookingsPath);

            Assert.Contains(reloadedCars.GetAll(), c => c.Model == car.Model && c.AvailabilityStatus == AvailabilityStatus.Booked);
            Assert.Contains(reloadedBookings.GetAll(), b => b.Car.Model == car.Model);
        }

        private static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "CarRentalTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
