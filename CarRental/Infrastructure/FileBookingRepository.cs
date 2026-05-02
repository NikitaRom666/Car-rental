using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    // Репозиторій бронювань збережений у файл (імітація)
    public class FileBookingRepository : IBookingRepository
    {
        private List<Booking> _bookings = new List<Booking>();
        private readonly string _filePath;

        public FileBookingRepository()
            : this(null)
        {
        }

        public FileBookingRepository(string? filePath)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "bookings.json"))
                : filePath;

            LoadAsync().GetAwaiter().GetResult();
        }

        public Booking? GetById(Guid id) => _bookings.FirstOrDefault(booking => booking.Id == id);

        public IEnumerable<Booking> GetAll() => _bookings;

        public void Add(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));
            if (_bookings.Any(existing => existing.Id == booking.Id))
                throw new InvalidOperationException("Бронювання з таким ID вже існує");

            _bookings.Add(booking);
            SaveAsync().GetAwaiter().GetResult();
        }

        public void Update(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            var index = _bookings.FindIndex(existing => existing.Id == booking.Id);
            if (index >= 0)
                _bookings[index] = booking;
            else
                _bookings.Add(booking);

            SaveAsync().GetAwaiter().GetResult();
        }

        public IEnumerable<Booking> GetByCar(Guid carId) =>
            _bookings.Where(booking => booking.Car.Id == carId);

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            await LoadAsync();
            return _bookings.FirstOrDefault(booking => booking.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetOverlappingAsync(Guid carId, DateOnly start, DateOnly end)
        {
            await LoadAsync();
            return _bookings.Where(booking => booking.Car.Id == carId &&
                booking.Period.Start < end &&
                booking.Period.End > start).ToList();
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            EnsureDirectory();
            if (!System.IO.File.Exists(_filePath))
            {
                _bookings = new List<Booking>();
                return;
            }

            try
            {
                await using var stream = System.IO.File.OpenRead(_filePath);
                var storage = await System.Text.Json.JsonSerializer.DeserializeAsync<List<BookingStorageModel>>(stream, cancellationToken: cancellationToken) ?? new List<BookingStorageModel>();
                _bookings = storage
                    .Where(item => item != null)
                    .GroupBy(item => item.Id)
                    .Select(group => FromStorageModel(group.First()))
                    .ToList();
            }
            catch
            {
                _bookings = new List<Booking>();
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureDirectory();
                var storage = _bookings.Select(ToStorageModel).ToList();
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                await using var stream = System.IO.File.Create(_filePath);
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, storage, options, cancellationToken);
            }
            catch
            {
                // Демо-варіант: помилку не кидаємо вгору, щоб консоль не падала.
            }
        }

        private void EnsureDirectory()
        {
            var directory = System.IO.Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
        }

        private static BookingStorageModel ToStorageModel(Booking booking)
        {
            return new BookingStorageModel
            {
                Id = booking.Id,
                CarId = booking.Car.Id,
                CarModel = booking.Car.Model,
                Category = booking.Car.Category,
                AvailabilityStatus = booking.Car.AvailabilityStatus,
                CustomerId = booking.Customer.Id,
                CustomerName = booking.Customer.Name,
                Start = booking.Period.Start,
                End = booking.Period.End,
                Price = booking.Price,
                Status = booking.Status
            };
        }

        private static Booking FromStorageModel(BookingStorageModel model)
        {
            var car = new Car(model.CarId, model.CarModel, model.Category, model.AvailabilityStatus);
            var customer = new Customer(model.CustomerId, model.CustomerName);
            var period = new BookingPeriod(model.Start, model.End);
            return new Booking(model.Id, car, customer, period, model.Price, model.Status);
        }

        private sealed class BookingStorageModel
        {
            public Guid Id { get; set; }
            public Guid CarId { get; set; }
            public string CarModel { get; set; } = string.Empty;
            public VehicleCategory Category { get; set; }
            public AvailabilityStatus AvailabilityStatus { get; set; }
            public Guid CustomerId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public DateOnly Start { get; set; }
            public DateOnly End { get; set; }
            public decimal Price { get; set; }
            public BookingStatus Status { get; set; }
        }
    }
}
