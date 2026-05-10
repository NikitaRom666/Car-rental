using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CarRental.Domain;
using CarRental.Infrastructure;
using Xunit;

namespace CarRental.Tests
{
    public class FilePersistenceFaultTests
    {
        [Fact]
        public void Load_CorruptedJson_DoesNotThrow_AndReturnsEmpty()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-corrupt-{Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path, "NOT A VALID JSON");

                var repo = new FileBookingRepository(path);

                var all = repo.GetAll();
                Assert.NotNull(all);
                Assert.Empty(all);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Add_AfterCorruptedJson_OverwritesFileWithValidJson()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-corrupt-{Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path, "TRUNCATED{ this is not valid json }");

                var repo = new FileBookingRepository(path);

                var car = new Car(Guid.NewGuid(), "Test Model", VehicleCategory.Economy, AvailabilityStatus.Available);
                var booking = new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Тест"), new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(2))), 100m);

                repo.Add(booking);

                // прочитати файл і переконатися, що в ньому валідний JSON з одним елементом
                var text = File.ReadAllText(path);
                var parsed = JsonSerializer.Deserialize<object[]>(text);
                Assert.NotNull(parsed);
                Assert.Single(parsed);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
