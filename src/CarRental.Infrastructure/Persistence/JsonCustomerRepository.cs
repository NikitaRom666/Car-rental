namespace CarRental.Infrastructure.Persistence;
using System.Text.Json;
using CarRental.Domain.Entities;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// JSON file-based repository for Customer entities.
/// </summary>
public class JsonCustomerRepository : ICustomerRepository
{
    private readonly string _filePath;
    private List<Customer> _customers;

    public JsonCustomerRepository(string dataDirectory = "data")
    {
        _filePath = Path.Combine(dataDirectory, "customers.json");
        _customers = new List<Customer>();
        LoadFromFile();
    }

    public Task<Customer?> GetByIdAsync(Guid id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(customer);
    }

    public Task<Customer?> GetByEmailAsync(string email)
    {
        var customer = _customers.FirstOrDefault(c => c.Email == email);
        return Task.FromResult(customer);
    }

    public Task<IEnumerable<Customer>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Customer>>(_customers.AsReadOnly());
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return Task.FromResult(_customers.Any(c => c.Id == id));
    }

    public Task AddAsync(Customer customer)
    {
        _customers.Add(customer);
        SaveToFile();
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {
        var index = _customers.FindIndex(c => c.Id == customer.Id);
        if (index >= 0)
        {
            _customers[index] = customer;
            SaveToFile();
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _customers.RemoveAll(c => c.Id == id);
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
                var dtos = JsonSerializer.Deserialize<List<CustomerJsonDto>>(json) ?? new();
                _customers = dtos.Select(dto => dto.ToCustomer()).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading customers from file: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var dtos = _customers.Select(CustomerJsonDto.FromCustomer).ToList();
            var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving customers to file: {ex.Message}");
        }
    }

    private class CustomerJsonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static CustomerJsonDto FromCustomer(Customer customer) => new()
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DriverLicenseNumber = customer.DriverLicenseNumber,
            CreatedAt = customer.CreatedAt
        };

        public Customer ToCustomer()
        {
            var customer = new Customer(Name, Email, PhoneNumber, DriverLicenseNumber);
            return customer;
        }
    }
}
