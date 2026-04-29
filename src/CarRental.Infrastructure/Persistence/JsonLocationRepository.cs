namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Location entities.
/// </summary>
public class JsonLocationRepository : ILocationRepository
{
    private readonly string _filePath;
    private List<Location> _locations;

    public JsonLocationRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "locations.json");
        _locations = new List<Location>();
        LoadFromFile();
    }

    public Task<Location?> GetByIdAsync(Guid id)
    {
        var location = _locations.FirstOrDefault(l => l.Id == id);
        return Task.FromResult(location);
    }

    public Task<IEnumerable<Location>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Location>>(_locations.AsReadOnly());
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return Task.FromResult(_locations.Any(l => l.Id == id));
    }

    public Task AddAsync(Location location)
    {
        _locations.Add(location);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Location location)
    {
        var index = _locations.FindIndex(l => l.Id == location.Id);
        if (index >= 0)
        {
            _locations[index] = location;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _locations.RemoveAll(l => l.Id == id);
        SaveToFile();
        return Task.CompletedTask;
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var dtos = JsonSerializer.Deserialize<List<LocationJsonDto>>(json) ?? new();
                _locations = dtos.Select(dto => dto.ToLocation()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading locations from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _locations.Select(LocationJsonDto.FromLocation).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving locations to file: {ex.Message}");
        }
    }

    private class LocationJsonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static LocationJsonDto FromLocation(Location location) => new()
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            PostalCode = location.PostalCode,
            CreatedAt = location.CreatedAt
        };

        public Location ToLocation()
        {
            var location = new Location(Name, Address, City, PostalCode);
            return location;
        }
    }
}
