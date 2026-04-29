namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Rental entities.
/// </summary>
public class JsonRentalRepository : IRentalRepository
{
    private readonly string _filePath;
    private List<Rental> _rentals;

    public JsonRentalRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "rentals.json");
        _rentals = new List<Rental>();
        LoadFromFile();
    }

    public Task<Rental?> GetByIdAsync(Guid id)
    {
        var rental = _rentals.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(rental);
    }

    public Task<IEnumerable<Rental>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Rental>>(_rentals.AsReadOnly());
    }

    public Task<IEnumerable<Rental>> GetByCustomerIdAsync(Guid customerId)
    {
        var rentals = _rentals.Where(r => r.CustomerId == customerId).ToList();
        return Task.FromResult<IEnumerable<Rental>>(rentals);
    }

    public Task<IEnumerable<Rental>> GetActiveRentalsForCarAsync(Guid carId)
    {
        var active = _rentals.Where(r => r.CarId == carId && 
                                         (int)r.Status == (int)Domain.Enums.RentalStatus.Active).ToList();
        return Task.FromResult<IEnumerable<Rental>>(active);
    }

    public Task<bool> HasConflictingRentalsAsync(Guid carId, DateRange period)
    {
        var conflicts = _rentals.Where(r => r.CarId == carId &&
                                           ((int)r.Status == (int)Domain.Enums.RentalStatus.Created ||
                                            (int)r.Status == (int)Domain.Enums.RentalStatus.Active))
                               .Any(r => r.RentalPeriod.OverlapsWith(period));
        return Task.FromResult(conflicts);
    }

    public Task AddAsync(Rental rental)
    {
        _rentals.Add(rental);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Rental rental)
    {
        var index = _rentals.FindIndex(r => r.Id == rental.Id);
        if (index >= 0)
        {
            _rentals[index] = rental;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _rentals.RemoveAll(r => r.Id == id);
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
                var dtos = JsonSerializer.Deserialize<List<RentalJsonDto>>(json) ?? new();
                _rentals = dtos.Select(dto => dto.ToRental()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rentals from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _rentals.Select(RentalJsonDto.FromRental).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving rentals to file: {ex.Message}");
        }
    }

    private class RentalJsonDto
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public static RentalJsonDto FromRental(Rental rental) => new()
        {
            Id = rental.Id,
            CarId = rental.CarId,
            CustomerId = rental.CustomerId,
            StartDate = rental.RentalPeriod.StartDate,
            EndDate = rental.RentalPeriod.EndDate,
            TotalPrice = rental.TotalPrice.Amount,
            Status = (int)rental.Status,
            CreatedAt = rental.CreatedAt,
            CompletedAt = rental.CompletedAt
        };

        public Rental ToRental()
        {
            var period = new DateRange(StartDate, EndDate);
            var totalPrice = new Money(TotalPrice);
            var rental = new Rental(CarId, CustomerId, period, totalPrice);
            
            // Restore status
            var status = (Domain.Enums.RentalStatus)Status;
            switch (status)
            {
                case Domain.Enums.RentalStatus.Active:
                    rental.Activate();
                    break;
                case Domain.Enums.RentalStatus.Completed:
                    rental.Activate();
                    rental.Complete();
                    break;
                case Domain.Enums.RentalStatus.Cancelled:
                    rental.Cancel();
                    break;
            }

            return rental;
        }
    }
}
