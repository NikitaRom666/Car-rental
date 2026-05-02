using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    // Репозиторій авто збережений у файл (імітація)
    public class FileCarRepository : ICarRepository
    {
        private List<Car> _cars;
        private readonly string _filePath;

        public FileCarRepository()
            : this(null)
        {
        }

        public FileCarRepository(string? filePath)
        {
            _filePath = string.IsNullOrWhiteSpace(filePath)
                ? System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "cars.json"))
                : filePath;

            _cars = new List<Car>();
            LoadAsync().GetAwaiter().GetResult();

            if (_cars.Count == 0)
            {
                _cars = new List<Car>
                {
                    new Car(Guid.NewGuid(), "Toyota Yaris", VehicleCategory.Economy, AvailabilityStatus.Available),
                    new Car(Guid.NewGuid(), "VW Passat", VehicleCategory.Standard, AvailabilityStatus.Available),
                    new Car(Guid.NewGuid(), "BMW X5", VehicleCategory.SUV, AvailabilityStatus.Available)
                };
                SaveAsync().GetAwaiter().GetResult();
            }
        }

        public Car? GetById(Guid id) => _cars.FirstOrDefault(car => car.Id == id);

        public IEnumerable<Car> GetAll() => _cars;

        public void Add(Car car)
        {
            if (car == null)
                throw new ArgumentNullException(nameof(car));
            if (_cars.Any(existing => existing.Id == car.Id))
                throw new InvalidOperationException("Авто з таким ID вже існує");

            _cars.Add(car);
            SaveAsync().GetAwaiter().GetResult();
        }

        public void Update(Car car)
        {
            if (car == null)
                throw new ArgumentNullException(nameof(car));

            var index = _cars.FindIndex(existing => existing.Id == car.Id);
            if (index >= 0)
                _cars[index] = car;
            else
                _cars.Add(car);

            SaveAsync().GetAwaiter().GetResult();
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            EnsureDirectory();
            if (!System.IO.File.Exists(_filePath))
            {
                _cars = new List<Car>();
                return;
            }

            try
            {
                await using var stream = System.IO.File.OpenRead(_filePath);
                var storage = await System.Text.Json.JsonSerializer.DeserializeAsync<List<CarStorageModel>>(stream, cancellationToken: cancellationToken) ?? new List<CarStorageModel>();
                _cars = storage
                    .Where(item => item != null)
                    .GroupBy(item => item.Id)
                    .Select(group => FromStorageModel(group.First()))
                    .ToList();
            }
            catch
            {
                _cars = new List<Car>();
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureDirectory();
                var storage = _cars.Select(ToStorageModel).ToList();
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

        private static CarStorageModel ToStorageModel(Car car)
        {
            return new CarStorageModel
            {
                Id = car.Id,
                Model = car.Model,
                Category = car.Category,
                AvailabilityStatus = car.AvailabilityStatus
            };
        }

        private static Car FromStorageModel(CarStorageModel model)
        {
            return new Car(model.Id, model.Model, model.Category, model.AvailabilityStatus);
        }

        private sealed class CarStorageModel
        {
            public Guid Id { get; set; }
            public string Model { get; set; } = string.Empty;
            public VehicleCategory Category { get; set; }
            public AvailabilityStatus AvailabilityStatus { get; set; }
        }
    }
}
