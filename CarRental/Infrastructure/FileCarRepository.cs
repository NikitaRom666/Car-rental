using System;
using System.Collections.Generic;
using System.Linq;
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

            try
            {
                LoadAsync().Wait();
            }
            catch
            {
                _cars = new List<Car>();
            }
            // Якщо після завантаження список порожній — ініціалізуємо дефолтними авто
            if (_cars == null || _cars.Count == 0)
            {
                _cars = new List<Car>
                {
                    new Car(Guid.NewGuid(), "Toyota Yaris", VehicleCategory.Economy, AvailabilityStatus.Available),
                    new Car(Guid.NewGuid(), "VW Passat", VehicleCategory.Standard, AvailabilityStatus.Available),
                    new Car(Guid.NewGuid(), "BMW X5", VehicleCategory.SUV, AvailabilityStatus.Available)
                };
                SaveAsync().Wait();
            }
        }

        public Car GetById(Guid id) => _cars.FirstOrDefault(c => c.Id == id);

        public IEnumerable<Car> GetAll() => _cars;

        public void Update(Car car)
        {
            var idx = _cars.FindIndex(c => c.Id == car.Id);
            if (idx >= 0)
                _cars[idx] = car;
            SaveAsync().Wait();
        }

        // Асинхронний пошук авто
        public async System.Threading.Tasks.Task<Car> GetByIdAsync(Guid id)
        {
            await LoadAsync();
            return _cars.FirstOrDefault(c => c.Id == id);
        }

        // Збереження у файл
        public async System.Threading.Tasks.Task SaveAsync()
        {
            try
            {
                EnsureDirectory();
                var storage = _cars.Select(ToStorageModel).ToList();
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
                _cars = new List<Car>();
                return;
            }
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(_filePath);
                var storage = System.Text.Json.JsonSerializer.Deserialize<List<CarStorageModel>>(json) ?? new List<CarStorageModel>();
                _cars = storage.Select(FromStorageModel).ToList();
            }
            catch
            {
                _cars = new List<Car>();
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
            public string Model { get; set; }
            public VehicleCategory Category { get; set; }
            public AvailabilityStatus AvailabilityStatus { get; set; }
        }
    }
}
