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
        public void Load_CorruptedJson_ThrowsPersistenceException()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-corrupt-{Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path, "NOT A VALID JSON");

                Assert.Throws<PersistenceException>(() => new FileBookingRepository(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Load_MissingFile_CreatesEmpty()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-missing-{Guid.NewGuid()}.json");
            try
            {
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
        public void Add_AfterCorruptedJson_ThrowsPersistenceException()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-corrupt-{Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path, "{ INVALID JSON }");

                Assert.Throws<PersistenceException>(() => new FileBookingRepository(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public async Task SaveAsync_ToReadOnlyFile_ThrowsPersistenceException()
        {
            var path = Path.Combine(Path.GetTempPath(), $"bookings-readonly-{Guid.NewGuid()}.json");
            try
            {
                File.WriteAllText(path, "[]");
                var fileInfo = new FileInfo(path);
                fileInfo.Attributes = FileAttributes.ReadOnly;

                var repo = new FileBookingRepository(path);
                var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
                var booking = new Booking(Guid.NewGuid(), car, new Customer(Guid.NewGuid(), "Test"), 
                    new BookingPeriod(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12)), 100m);

                await Assert.ThrowsAsync<PersistenceException>(() => repo.SaveAsync());
            }
            finally
            {
                if (File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    fileInfo.Attributes = FileAttributes.Normal;
                    File.Delete(path);
                }
            }
        }
    }
}
