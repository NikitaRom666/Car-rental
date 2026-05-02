using System;
using System.Collections.Generic;
using System.Linq;
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

            try { LoadAsync().Wait(); }
            catch { _bookings = new List<Booking>(); }
        }

        public IEnumerable<Booking> GetAll() => _bookings;

        public void Add(Booking booking)
        {
            _bookings.Add(booking);
            SaveAsync().Wait();
        }

        public IEnumerable<Booking> GetByCar(Guid carId) =>
            _bookings.Where(b => b.Car.Id == carId);

        // Асинхронний пошук бронювання за авто
        public async System.Threading.Tasks.Task<Booking> GetByIdAsync(Guid id)
        {
            await LoadAsync();
            return _bookings.FirstOrDefault(b => b.Id == id);
        }

        // Пошук перетинів бронювань
        public async System.Threading.Tasks.Task<IEnumerable<Booking>> GetOverlappingAsync(Guid carId, DateOnly start, DateOnly end)
        {
            await LoadAsync();
            return _bookings.Where(b => b.Car.Id == carId &&
                b.Period.Start < end &&
                b.Period.End > start).ToList();
        }

        // Збереження у файл
        public async System.Threading.Tasks.Task SaveAsync()
        {
            try
            {
                EnsureDirectory();
                var storage = _bookings.Select(ToStorageModel).ToList();
                var json = System.Text.Json.JsonSerializer.Serialize(storage);
                await System.IO.File.WriteAllTextAsync(_filePath, json);
            }
            catch { /* ignore */ }
        }

        // Завантаження з файлу
        public async System.Threading.Tasks.Task LoadAsync()
        {
            EnsureDirectory();
            if (!System.IO.File.Exists(_filePath))
            {
                _bookings = new List<Booking>();
                return;
            }
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(_filePath);
                var storage = System.Text.Json.JsonSerializer.Deserialize<List<BookingStorageModel>>(json) ?? new List<BookingStorageModel>();
                _bookings = storage.Select(FromStorageModel).ToList();
            }
            catch
            {
                _bookings = new List<Booking>();
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
                Price = booking.Price
            };
        }

        private static Booking FromStorageModel(BookingStorageModel model)
        {
            var car = new Car(model.CarId, model.CarModel, model.Category, model.AvailabilityStatus);
            var customer = new Customer(model.CustomerId, model.CustomerName);
            var period = new BookingPeriod(model.Start, model.End);
            return new Booking(model.Id, car, customer, period, model.Price);
        }

        private sealed class BookingStorageModel
        {
            public Guid Id { get; set; }
            public Guid CarId { get; set; }
            public string CarModel { get; set; }
            public VehicleCategory Category { get; set; }
            public AvailabilityStatus AvailabilityStatus { get; set; }
            public Guid CustomerId { get; set; }
            public string CustomerName { get; set; }
            public DateOnly Start { get; set; }
            public DateOnly End { get; set; }
            public decimal Price { get; set; }
        }
    }
}
