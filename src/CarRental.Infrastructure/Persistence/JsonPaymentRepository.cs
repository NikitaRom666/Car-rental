namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Payment entities.
/// </summary>
public class JsonPaymentRepository : IPaymentRepository
{
    private readonly string _filePath;
    private List<Payment> _payments;

    public JsonPaymentRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "payments.json");
        _payments = new List<Payment>();
        LoadFromFile();
    }

    public Task<Payment?> GetByIdAsync(Guid id)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(payment);
    }

    public Task<IEnumerable<Payment>> GetByRentalIdAsync(Guid rentalId)
    {
        var payments = _payments.Where(p => p.RentalId == rentalId).ToList();
        return Task.FromResult<IEnumerable<Payment>>(payments);
    }

    public Task<IEnumerable<Payment>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Payment>>(_payments.AsReadOnly());
    }

    public Task AddAsync(Payment payment)
    {
        _payments.Add(payment);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Payment payment)
    {
        var index = _payments.FindIndex(p => p.Id == payment.Id);
        if (index >= 0)
        {
            _payments[index] = payment;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _payments.RemoveAll(p => p.Id == id);
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
                var dtos = JsonSerializer.Deserialize<List<PaymentJsonDto>>(json) ?? new();
                _payments = dtos.Select(dto => dto.ToPayment()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading payments from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _payments.Select(PaymentJsonDto.FromPayment).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving payments to file: {ex.Message}");
        }
    }

    private class PaymentJsonDto
    {
        public Guid Id { get; set; }
        public Guid RentalId { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionReference { get; set; }

        public static PaymentJsonDto FromPayment(Payment payment) => new()
        {
            Id = payment.Id,
            RentalId = payment.RentalId,
            Amount = payment.Amount.Amount,
            Status = (int)payment.Status,
            CreatedAt = payment.CreatedAt,
            PaidAt = payment.PaidAt,
            TransactionReference = payment.TransactionReference
        };

        public Payment ToPayment()
        {
            var amount = new Money(Amount);
            var payment = new Payment(RentalId, amount);
            
            var status = (Domain.Enums.PaymentStatus)Status;
            if (status == Domain.Enums.PaymentStatus.Paid && TransactionReference != null)
                payment.MarkAsPaid(TransactionReference);
            else if (status == Domain.Enums.PaymentStatus.Failed)
                payment.MarkAsFailed();

            return payment;
        }
    }
}
