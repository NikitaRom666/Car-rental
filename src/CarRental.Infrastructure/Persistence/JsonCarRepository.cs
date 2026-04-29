namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Car entities.
/// Persists data to /data/cars.json file.
/// </summary>
public class JsonCarRepository : ICarRepository
{
    private readonly string _filePath;
    private List<Car> _cars;

    public JsonCarRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "cars.json");
        _cars = new List<Car>();
        LoadFromFile();
    }

    public Task<Car?> GetByIdAsync(Guid id)
    {
        var car = _cars.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(car);
    }

    public Task<IEnumerable<Car>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Car>>(_cars.AsReadOnly());
    }

    public Task<IEnumerable<Car>> GetAvailableCarsAsync()
    {
        var available = _cars.Where(c => c.IsAvailable).ToList();
        return Task.FromResult<IEnumerable<Car>>(available);
    }

    public Task<IEnumerable<Car>> GetCarsByCategoryAsync(int category)
    {
        var byCat = _cars.Where(c => (int)c.Category == category).ToList();
        return Task.FromResult<IEnumerable<Car>>(byCat);
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return Task.FromResult(_cars.Any(c => c.Id == id));
    }

    public Task AddAsync(Car car)
    {
        _cars.Add(car);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Car car)
    {
        var index = _cars.FindIndex(c => c.Id == car.Id);
        if (index >= 0)
        {
            _cars[index] = car;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _cars.RemoveAll(c => c.Id == id);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Car>> GetCarsAvailableForPeriodAsync(DateRange period)
    {
        // In real scenario, would check conflicts with rentals
        var available = _cars.Where(c => c.IsAvailable).ToList();
        return Task.FromResult<IEnumerable<Car>>(available);
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var dtos = JsonSerializer.Deserialize<List<CarJsonDto>>(json) ?? new();
                _cars = dtos.Select(dto => dto.ToCar()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cars from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _cars.Select(CarJsonDto.FromCar).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving cars to file: {ex.Message}");
        }
    }

    /// <summary>
    /// JSON DTO for serialization (internal use).
    /// </summary>
    private class CarJsonDto
    {
        public Guid Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public int Category { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsAvailable { get; set; }
        public Guid LocationId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public int ManufacturingYear { get; set; }
        public DateTime CreatedAt { get; set; }

        public static CarJsonDto FromCar(Car car) => new()
        {
            Id = car.Id,
            Model = car.Model,
            Category = (int)car.Category,
            PricePerDay = car.PricePerDay.Amount,
            IsAvailable = car.IsAvailable,
            LocationId = car.LocationId,
            LicensePlate = car.LicensePlate,
            ManufacturingYear = car.ManufacturingYear,
            CreatedAt = car.CreatedAt
        };

        public Car ToCar()
        {
            var pricePerDay = new Money(PricePerDay);
            var car = new Car(Model, (Domain.Enums.CarCategory)Category, pricePerDay, LocationId, 
                            LicensePlate, ManufacturingYear);
            return car;
        }
    }
}
