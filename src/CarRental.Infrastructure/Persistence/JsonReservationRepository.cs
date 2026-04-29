namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Reservation entities.
/// </summary>
public class JsonReservationRepository : IReservationRepository
{
    private readonly string _filePath;
    private List<Reservation> _reservations;

    public JsonReservationRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "reservations.json");
        _reservations = new List<Reservation>();
        LoadFromFile();
    }

    public Task<Reservation?> GetByIdAsync(Guid id)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(reservation);
    }

    public Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Reservation>>(_reservations.AsReadOnly());
    }

    public Task<IEnumerable<Reservation>> GetByCarIdAsync(Guid carId)
    {
        var reservations = _reservations.Where(r => r.CarId == carId).ToList();
        return Task.FromResult<IEnumerable<Reservation>>(reservations);
    }

    public Task<IEnumerable<Reservation>> GetActiveReservationsForCarAsync(Guid carId)
    {
        var active = _reservations.Where(r => r.CarId == carId && r.IsActive).ToList();
        return Task.FromResult<IEnumerable<Reservation>>(active);
    }

    public Task<bool> HasConflictingReservationsAsync(Guid carId, DateRange period)
    {
        var conflicts = _reservations.Where(r => r.CarId == carId && r.IsActive)
                                     .Any(r => r.ReservationPeriod.OverlapsWith(period));
        return Task.FromResult(conflicts);
    }

    public Task AddAsync(Reservation reservation)
    {
        _reservations.Add(reservation);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Reservation reservation)
    {
        var index = _reservations.FindIndex(r => r.Id == reservation.Id);
        if (index >= 0)
        {
            _reservations[index] = reservation;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _reservations.RemoveAll(r => r.Id == id);
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
                var dtos = JsonSerializer.Deserialize<List<ReservationJsonDto>>(json) ?? new();
                _reservations = dtos.Select(dto => dto.ToReservation()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reservations from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _reservations.Select(ReservationJsonDto.FromReservation).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving reservations to file: {ex.Message}");
        }
    }

    private class ReservationJsonDto
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public static ReservationJsonDto FromReservation(Reservation reservation) => new()
        {
            Id = reservation.Id,
            CarId = reservation.CarId,
            CustomerId = reservation.CustomerId,
            StartDate = reservation.ReservationPeriod.StartDate,
            EndDate = reservation.ReservationPeriod.EndDate,
            IsActive = reservation.IsActive,
            CreatedAt = reservation.CreatedAt,
            CancelledAt = reservation.CancelledAt
        };

        public Reservation ToReservation()
        {
            var period = new DateRange(StartDate, EndDate);
            var reservation = new Reservation(CarId, CustomerId, period);
            
            if (!IsActive)
                reservation.Cancel();

            return reservation;
        }
    }
}
