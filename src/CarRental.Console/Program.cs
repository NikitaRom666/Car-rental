namespace CarRental.Console;
using Microsoft.Extensions.DependencyInjection;
using CarRental.Infrastructure;
using CarRental.Application.Services;
using CarRental.Application.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;
using CarRental.Application.Interfaces.Repositories;

/// <summary>
/// Entry point for the Car Rental System console application.
/// Demonstrates dependency injection, clean architecture, and system usage.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        
        // Register infrastructure services (repositories, observers)
        services.AddInfrastructureServices("data");
        
        // Register application services
        services.AddScoped(provider =>
            new RentalService(
                provider.GetRequiredService<ICarRepository>(),
                provider.GetRequiredService<ICustomerRepository>(),
                provider.GetRequiredService<IRentalRepository>(),
                provider.GetRequiredService<IReservationRepository>(),
                provider.GetRequiredService<IPaymentRepository>(),
                provider.GetRequiredService<ILocationRepository>(),
                provider.GetRequiredService<IRentalEventObserver>()
            )
        );

        var serviceProvider = services.BuildServiceProvider();

        // Initialize sample data if needed
        await InitializeSampleDataAsync(serviceProvider);

        // Start CLI menu
        var app = new ConsoleApplication(serviceProvider);
        await app.RunAsync();
    }

    private static async Task InitializeSampleDataAsync(IServiceProvider serviceProvider)
    {
        var carRepo = serviceProvider.GetRequiredService<ICarRepository>();
        var customerRepo = serviceProvider.GetRequiredService<ICustomerRepository>();
        var locationRepo = serviceProvider.GetRequiredService<ILocationRepository>();

        var cars = await carRepo.GetAllAsync();
        if (!cars.Any())
        {
            System.Console.WriteLine("Initializing sample data...");

            // Create location
            var location = new Location("Downtown Branch", "123 Main St", "New York", "10001");
            await locationRepo.AddAsync(location);

            // Create sample cars
            var car1 = new Car("Toyota Corolla", CarCategory.Economy, 
                new Money(50m), location.Id, "NY-001", 2023);
            var car2 = new Car("BMW 3 Series", CarCategory.Business, 
                new Money(100m), location.Id, "NY-002", 2023);
            var car3 = new Car("Mercedes-Benz S Class", CarCategory.Premium, 
                new Money(200m), location.Id, "NY-003", 2023);

            await carRepo.AddAsync(car1);
            await carRepo.AddAsync(car2);
            await carRepo.AddAsync(car3);

            // Create sample customer
            var customer = new Customer("John Doe", "john@example.com", "555-1234", "DL123456");
            await customerRepo.AddAsync(customer);

            System.Console.WriteLine("Sample data initialized successfully!");
        }
    }
}

/// <summary>
/// Console application that handles user interface and menu navigation.
/// Implements Clean Architecture by using only interfaces and DTOs.
/// </summary>
public class ConsoleApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RentalService _rentalService;
    private readonly ICarRepository _carRepository;
    private readonly ICustomerRepository _customerRepository;

    public ConsoleApplication(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _rentalService = serviceProvider.GetRequiredService<RentalService>();
        _carRepository = serviceProvider.GetRequiredService<ICarRepository>();
        _customerRepository = serviceProvider.GetRequiredService<ICustomerRepository>();
    }

    public async Task RunAsync()
    {
        bool running = true;
        while (running)
        {
            DisplayMainMenu();
            string? choice = System.Console.ReadLine();

            try
            {
                switch (choice?.Trim().ToUpper())
                {
                    case "1":
                        await ShowAvailableCarsAsync();
                        break;
                    case "2":
                        await CreateReservationAsync();
                        break;
                    case "3":
                        await StartRentalAsync();
                        break;
                    case "4":
                        await FinishRentalAsync();
                        break;
                    case "5":
                        await ShowAllCarsAsync();
                        break;
                    case "6":
                        await ShowAllCustomersAsync();
                        break;
                    case "0":
                        running = false;
                        System.Console.WriteLine("Thank you for using Car Rental System. Goodbye!");
                        break;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }

            if (running)
            {
                System.Console.WriteLine("\nPress Enter to continue...");
                System.Console.ReadLine();
            }
        }
    }

    private void DisplayMainMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("    WELCOME TO CAR RENTAL SYSTEM");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("1. Show Available Cars");
        System.Console.WriteLine("2. Create Reservation");
        System.Console.WriteLine("3. Start Rental");
        System.Console.WriteLine("4. Finish Rental");
        System.Console.WriteLine("5. Show All Cars");
        System.Console.WriteLine("6. Show All Customers");
        System.Console.WriteLine("0. Exit");
        System.Console.WriteLine("===========================================");
        System.Console.Write("Select option: ");
    }

    private async Task ShowAvailableCarsAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("AVAILABLE CARS");
        System.Console.WriteLine("==============");

        var cars = await _rentalService.GetAvailableCarsAsync();
        if (!cars.Any())
        {
            System.Console.WriteLine("No available cars at the moment.");
            return;
        }

        int index = 1;
        foreach (var car in cars)
        {
            System.Console.WriteLine($"\n{index}. Model: {car.Model}");
            System.Console.WriteLine($"   Category: {car.CategoryName}");
            System.Console.WriteLine($"   Price: ${car.PricePerDay}/day");
            System.Console.WriteLine($"   License Plate: {car.LicensePlate}");
            System.Console.WriteLine($"   Status: {car.AvailabilityStatus}");
            index++;
        }
    }

    private async Task ShowAllCarsAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("ALL CARS");
        System.Console.WriteLine("========");

        var cars = await _rentalService.GetAllCarsAsync();
        if (!cars.Any())
        {
            System.Console.WriteLine("No cars in the system.");
            return;
        }

        int index = 1;
        foreach (var car in cars)
        {
            System.Console.WriteLine($"\n{index}. Model: {car.Model} ({car.CategoryName})");
            System.Console.WriteLine($"   Price: ${car.PricePerDay}/day");
            System.Console.WriteLine($"   License: {car.LicensePlate}");
            System.Console.WriteLine($"   Status: {car.AvailabilityStatus}");
            index++;
        }
    }

    private async Task ShowAllCustomersAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("ALL CUSTOMERS");
        System.Console.WriteLine("=============");

        var customers = await _customerRepository.GetAllAsync();
        if (!customers.Any())
        {
            System.Console.WriteLine("No customers in the system.");
            return;
        }

        int index = 1;
        foreach (var customer in customers)
        {
            System.Console.WriteLine($"\n{index}. Name: {customer.Name}");
            System.Console.WriteLine($"   Email: {customer.Email}");
            System.Console.WriteLine($"   Phone: {customer.PhoneNumber}");
            index++;
        }
    }

    private async Task CreateReservationAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("CREATE RESERVATION");
        System.Console.WriteLine("==================");

        try
        {
            System.Console.Write("Enter Customer Email: ");
            var email = System.Console.ReadLine()?.Trim() ?? throw new InvalidOperationException("Email required");

            var customer = await _customerRepository.GetByEmailAsync(email)
                ?? throw new InvalidOperationException("Customer not found");

            System.Console.Write("Enter Car Model (partial): ");
            var model = System.Console.ReadLine()?.Trim() ?? throw new InvalidOperationException("Model required");

            var cars = await _rentalService.GetAllCarsAsync();
            var car = cars.FirstOrDefault(c => c.Model.Contains(model, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Car not found");

            System.Console.Write("Enter Start Date (yyyy-MM-dd): ");
            var startDate = DateTime.ParseExact(System.Console.ReadLine()?.Trim() ?? "", "yyyy-MM-dd", null);

            System.Console.Write("Enter End Date (yyyy-MM-dd): ");
            var endDate = DateTime.ParseExact(System.Console.ReadLine()?.Trim() ?? "", "yyyy-MM-dd", null);

            var request = new CarRental.Application.DTOs.CreateReservationRequest
            {
                CarId = car.Id,
                CustomerId = customer.Id,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _rentalService.CreateReservationAsync(request);

            if (result.Success)
            {
                System.Console.WriteLine($"\n✓ {result.Message}");
                System.Console.WriteLine($"Reservation ID: {result.Data?.Id}");
            }
            else
            {
                System.Console.WriteLine($"\n✗ {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task StartRentalAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("START RENTAL");
        System.Console.WriteLine("============");

        try
        {
            System.Console.Write("Enter Customer Email: ");
            var email = System.Console.ReadLine()?.Trim() ?? throw new InvalidOperationException("Email required");

            var customer = await _customerRepository.GetByEmailAsync(email)
                ?? throw new InvalidOperationException("Customer not found");

            System.Console.Write("Enter Car Model (partial): ");
            var model = System.Console.ReadLine()?.Trim() ?? throw new InvalidOperationException("Model required");

            var cars = await _rentalService.GetAllCarsAsync();
            var car = cars.FirstOrDefault(c => c.Model.Contains(model, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Car not found");

            System.Console.Write("Enter Start Date (yyyy-MM-dd): ");
            var startDate = DateTime.ParseExact(System.Console.ReadLine()?.Trim() ?? "", "yyyy-MM-dd", null);

            System.Console.Write("Enter End Date (yyyy-MM-dd): ");
            var endDate = DateTime.ParseExact(System.Console.ReadLine()?.Trim() ?? "", "yyyy-MM-dd", null);

            var request = new CarRental.Application.DTOs.CreateRentalRequest
            {
                CarId = car.Id,
                CustomerId = customer.Id,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _rentalService.StartRentalAsync(request);

            if (result.Success)
            {
                System.Console.WriteLine($"\n✓ {result.Message}");
                System.Console.WriteLine($"Rental ID: {result.Data?.Id}");
                System.Console.WriteLine($"Total Price: ${result.Data?.TotalPrice:F2}");
            }
            else
            {
                System.Console.WriteLine($"\n✗ {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task FinishRentalAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("FINISH RENTAL");
        System.Console.WriteLine("=============");

        try
        {
            System.Console.Write("Enter Rental ID: ");
            if (!Guid.TryParse(System.Console.ReadLine()?.Trim(), out var rentalId))
                throw new InvalidOperationException("Invalid Rental ID format");

            var result = await _rentalService.FinishRentalAsync(rentalId);

            if (result.Success)
            {
                System.Console.WriteLine($"\n✓ {result.Message}");
                System.Console.WriteLine($"Rental Status: {result.Data?.StatusName}");
            }
            else
            {
                System.Console.WriteLine($"\n✗ {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
