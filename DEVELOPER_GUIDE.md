# Developer Guide

## Introduction

This guide is for developers working on or extending the Car Rental System. It covers architecture, key patterns, code organization, and development workflows.

## Project Structure

```
CarRental/
├── src/
│   ├── CarRental.Domain/           # Business logic (Entities, Value Objects)
│   ├── CarRental.Application/      # Use cases, Services, DTOs
│   ├── CarRental.Infrastructure/   # Repositories, Persistence
│   └── CarRental.Console/          # CLI, UI, DI setup
├── tests/
│   └── CarRental.Tests/            # Unit and integration tests
├── data/                           # JSON data files
└── docs/                           # Documentation
```

## Getting Started

### Prerequisites
- .NET 6 SDK or higher
- C# 10 compatible IDE (VS Code, Visual Studio)
- Git for version control

### Initial Setup
```bash
# Clone repository
git clone <repo-url>
cd CarRental

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd src/CarRental.Console
dotnet run
```

## Architecture Overview

### Clean Architecture Layers

```
Dependency Flow (Arrows point inward)
┌────────────────────────────────────┐
│   Presentation Layer (Console UI)  │ ○ Framework dependencies only
├────────────────────────────────────┤
│   Application Layer                │ ○ Use cases, Services
├────────────────────────────────────┤
│   Infrastructure Layer             │ ○ Data access, File I/O
├────────────────────────────────────┤
│   Domain Layer (Core Business)     │ ○ Zero external dependencies
└────────────────────────────────────┘
```

**Key Rule:** Dependencies flow INWARD. Domain layer depends on nothing.

### Domain Layer (`Domain/`)

**Responsibility:** Encapsulate all business logic and rules.

**Contents:**
- `Entities/` - Domain objects with identity (Car, Rental, Customer)
- `ValueObjects/` - Immutable domain concepts (Money, DateRange)
- `Enums/` - Domain constants (CarCategory, RentalStatus)
- `Exceptions/` - Domain-specific exceptions

**Principles:**
- Rich entities with behavior (not anemic data models)
- Validation in constructors
- Immutable unless specifically required to change
- No null references (validate in constructor)
- Uses only System namespace (no external dependencies)

**Example - Car Entity:**
```csharp
public class Car
{
    public Guid Id { get; private set; }
    public string Model { get; private set; }
    public bool IsAvailable { get; private set; }

    // Constructor with validation
    public Car(string model, CarCategory category, Money pricePerDay, ...)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model required");
        
        Model = model;
        // ... other assignments with validation
    }

    // Methods encapsulating business logic
    public void MakeUnavailable()
    {
        IsAvailable = false;
    }

    public void ValidateAvailability()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Car not available");
    }
}
```

### Application Layer (`Application/`)

**Responsibility:** Orchestrate use cases and coordinate between domain and infrastructure.

**Contents:**
- `DTOs/` - Data transfer objects for layer boundaries
- `Interfaces/` - Repository and service contracts
- `Services/` - Business orchestration (RentalService)

**Principles:**
- Depends on domain via interfaces
- Uses DTOs for input/output (not domain entities)
- Service methods represent use cases
- No direct database/file access

**Example - RentalService:**
```csharp
public class RentalService
{
    private readonly ICarRepository _carRepository;
    private readonly IRentalRepository _rentalRepository;
    // ... other dependencies

    public async Task<OperationResult<RentalDto>> StartRentalAsync(CreateRentalRequest request)
    {
        // 1. Validate inputs exist
        var car = await _carRepository.GetByIdAsync(request.CarId);
        if (car == null) throw new NotFoundException(nameof(Car), request.CarId);

        // 2. Check business rules
        car.ValidateAvailability();
        bool isAvailable = await CheckCarAvailabilityAsync(...);
        if (!isAvailable) throw new BusinessRuleViolationException(...);

        // 3. Execute business logic
        var rental = new Rental(request.CarId, request.CustomerId, period, totalPrice);
        rental.Activate();
        car.MakeUnavailable();

        // 4. Persist changes
        await _rentalRepository.AddAsync(rental);
        await _carRepository.UpdateAsync(car);

        // 5. Return DTO
        return OperationResult<RentalDto>.CreateSuccess(..., MapToDto(rental));
    }
}
```

### Infrastructure Layer (`Infrastructure/`)

**Responsibility:** Concrete implementations of persistence and external services.

**Contents:**
- `Persistence/` - Repository implementations (JSON-based)
- `InfrastructureServiceCollectionExtensions.cs` - DI setup

**Principles:**
- Implements interfaces from Application layer
- Handles file I/O, serialization
- Swappable implementations (JSON → SQL, etc.)

**Example - JsonCarRepository:**
```csharp
public class JsonCarRepository : ICarRepository
{
    private readonly string _filePath;
    private List<Car> _cars;

    public async Task AddAsync(Car car)
    {
        _cars.Add(car);
        SaveToFile();  // Persist to cars.json
    }

    private void SaveToFile()
    {
        var dtos = _cars.Select(CarJsonDto.FromCar).ToList();
        var json = JsonSerializer.Serialize(dtos, ...);
        File.WriteAllText(_filePath, json);
    }
}
```

### Console UI Layer (`Console/`)

**Responsibility:** User interface and dependency injection setup.

**Contents:**
- `Program.cs` - Entry point, DI configuration, CLI menu

**Principles:**
- No business logic here (all in services)
- Menu-driven navigation
- Error handling and user feedback
- DI container bootstrap

## Key Design Patterns

### 1. Repository Pattern

**Purpose:** Abstract data access behind interfaces

**Usage:**
```csharp
// Interface (Application layer)
public interface ICarRepository
{
    Task<Car?> GetByIdAsync(Guid id);
    Task AddAsync(Car car);
}

// Implementation (Infrastructure layer)
public class JsonCarRepository : ICarRepository
{
    public async Task<Car?> GetByIdAsync(Guid id)
    {
        return _cars.FirstOrDefault(c => c.Id == id);
    }
}

// Usage (Application layer)
public class RentalService
{
    public RentalService(ICarRepository carRepository)
    {
        _carRepository = carRepository;  // Depends on interface, not implementation
    }
}
```

**Benefits:**
- Swappable implementations (JSON ↔ SQL)
- Testable with mocks
- Clear separation of concerns

### 2. Strategy Pattern (Pricing)

**Purpose:** Encapsulate pricing algorithms

**Usage:**
```csharp
// Strategy interface
public interface IPricingStrategy
{
    CarCategory Category { get; }
    Money CalculatePrice(Money dailyRate, int daysCount);
}

// Concrete strategies
public class EconomyPricingStrategy : IPricingStrategy
{
    public Money CalculatePrice(Money dailyRate, int daysCount)
    {
        return dailyRate.Multiply(daysCount);  // No discount
    }
}

public class PremiumPricingStrategy : IPricingStrategy
{
    public Money CalculatePrice(Money dailyRate, int daysCount)
    {
        // Progressive discounts
        var discountPercentage = daysCount switch
        {
            <= 3 => 0m,
            <= 7 => 0.05m,
            <= 14 => 0.10m,
            _ => 0.15m
        };
        // ... calculate with discount
    }
}

// Factory registration
var strategies = new Dictionary<int, IPricingStrategy>
{
    { (int)CarCategory.Economy, new EconomyPricingStrategy() },
    { (int)CarCategory.Business, new BusinessPricingStrategy() },
    { (int)CarCategory.Premium, new PremiumPricingStrategy() }
};

// Usage
var strategy = strategies[(int)car.Category];
var price = strategy.CalculatePrice(car.PricePerDay, rentalPeriod.DaysCount);
```

**Benefits:**
- Add new pricing logic without modifying existing code (Open/Closed)
- Easy to test each strategy in isolation
- Clear separation of pricing logic

### 3. Observer Pattern (Events)

**Purpose:** Decouple event producers from event handlers

**Usage:**
```csharp
// Observer interface
public interface IRentalEventObserver
{
    Task OnRentalCreatedAsync(Guid rentalId, Guid customerId, Guid carId);
    Task OnRentalStartedAsync(Guid rentalId);
}

// Concrete observer
public class ConsoleRentalEventObserver : IRentalEventObserver
{
    public Task OnRentalCreatedAsync(Guid rentalId, Guid customerId, Guid carId)
    {
        Console.WriteLine($"[EVENT] Rental created: {rentalId}");
        return Task.CompletedTask;
    }
}

// Using observer
public class RentalService
{
    private readonly IRentalEventObserver _observer;
    
    public async Task<OperationResult<RentalDto>> StartRentalAsync(...)
    {
        var rental = new Rental(...);
        rental.Activate();
        
        await _observer.OnRentalStartedAsync(rental.Id);  // Notify
        
        return OperationResult<RentalDto>.CreateSuccess(...);
    }
}
```

**Benefits:**
- Decouple logging from core logic
- Multiple observers can listen
- Easy to add new observers without changing RentalService

### 4. Dependency Injection

**Purpose:** Manage object creation and lifetime

**Setup in Program.cs:**
```csharp
var services = new ServiceCollection();

// Register repositories (singleton - shared instance)
services.AddSingleton<ICarRepository>(new JsonCarRepository("data"));

// Register services (scoped - one per request)
services.AddScoped(provider =>
    new RentalService(
        provider.GetRequiredService<ICarRepository>(),
        provider.GetRequiredService<ICustomerRepository>(),
        // ... other dependencies
    )
);

var serviceProvider = services.BuildServiceProvider();
```

**Benefits:**
- Central configuration
- Automatic dependency resolution
- Easy to swap implementations

## Common Development Tasks

### Adding a New Feature

1. **Define Domain Model** (Domain/)
   ```csharp
   // New entity with validation
   public class Insurance
   {
       public decimal DailyRate { get; private set; }
       public string Type { get; private set; }
       
       public Insurance(string type, decimal dailyRate)
       {
           if (dailyRate < 0) throw new ArgumentException("Rate cannot be negative");
           Type = type;
           DailyRate = dailyRate;
       }
   }
   ```

2. **Create Repository Interface** (Application/)
   ```csharp
   public interface IInsuranceRepository
   {
       Task<Insurance?> GetByIdAsync(Guid id);
       Task AddAsync(Insurance insurance);
   }
   ```

3. **Implement Repository** (Infrastructure/)
   ```csharp
   public class JsonInsuranceRepository : IInsuranceRepository
   {
       // Load/save JSON implementation
   }
   ```

4. **Add Service Method** (Application/)
   ```csharp
   public class RentalService
   {
       public Money CalculateInsuranceCost(Rental rental, Insurance insurance)
       {
           return insurance.DailyRate * rental.RentalPeriod.DaysCount;
       }
   }
   ```

5. **Add UI** (Console/)
   ```csharp
   _insuranceService.SelectInsurance(rentalId);
   ```

6. **Add Tests** (Tests/)
   ```csharp
   [Fact]
   public void Insurance_CalculatesCostCorrectly() { }
   ```

### Modifying Entity Validation

**Current Validation (Car creation):**
```csharp
public Car(string model, CarCategory category, ...)
{
    if (string.IsNullOrWhiteSpace(model))
        throw new ArgumentException("Model required");
    // ... other validations
}
```

**Adding new validation:**
```csharp
public Car(string model, CarCategory category, ...)
{
    if (string.IsNullOrWhiteSpace(model))
        throw new ArgumentException("Model required");
    
    if (model.Length > 100)  // NEW
        throw new ArgumentException("Model too long");
    
    // ... other validations
}
```

**Add corresponding test:**
```csharp
[Fact]
public void Car_ThrowsWhenModelTooLong()
{
    var longModel = new string('A', 101);
    Assert.Throws<ArgumentException>(() =>
        new Car(longModel, CarCategory.Economy, ...)
    );
}
```

### Adding a New Repository

1. **Create interface in Application/Interfaces/Repositories:**
   ```csharp
   public interface IInsuranceRepository { }
   ```

2. **Implement in Infrastructure/Persistence:**
   ```csharp
   public class JsonInsuranceRepository : IInsuranceRepository { }
   ```

3. **Register in DI (InfrastructureServiceCollectionExtensions):**
   ```csharp
   services.AddSingleton<IInsuranceRepository>(
       new JsonInsuranceRepository(dataDirectory)
   );
   ```

4. **Use in services:**
   ```csharp
   public class RentalService
   {
       private readonly IInsuranceRepository _insuranceRepository;
   }
   ```

## Code Style Guide

### Naming Conventions

```csharp
// Classes: PascalCase
public class RentalService { }

// Methods: PascalCase
public async Task<Rental?> GetRentalAsync(Guid id) { }

// Parameters: camelCase
public void StartRental(Guid rentalId, CreateRentalRequest request) { }

// Private fields: _camelCase
private readonly ICarRepository _carRepository;

// Constants: UPPER_SNAKE_CASE (or PascalCase in .NET style)
private const decimal MAX_PRICE = 10000m;

// Interfaces: IPascalCase
public interface ICarRepository { }

// Enums: PascalCase values
public enum RentalStatus { Created, Active, Completed }
```

### Documentation Comments

```csharp
/// <summary>
/// Starts an active rental for a customer and vehicle.
/// Changes vehicle to unavailable and creates payment record.
/// </summary>
/// <param name="request">Rental request with car, customer, and dates</param>
/// <returns>Operation result with rental DTO or error message</returns>
/// <exception cref="NotFoundException">If car or customer not found</exception>
/// <exception cref="BusinessRuleViolationException">If business rules violated</exception>
public async Task<OperationResult<RentalDto>> StartRentalAsync(CreateRentalRequest request)
{
}
```

### Exception Handling

```csharp
// DO: Specific exceptions with context
try
{
    var car = await _carRepository.GetByIdAsync(carId)
        ?? throw new NotFoundException(nameof(Car), carId);
}
catch (NotFoundException ex)
{
    _logger.LogWarning($"Car not found: {ex.Message}");
    return OperationResult.CreateFailure(ex.Message);
}

// DON'T: Generic exceptions
catch (Exception ex)
{
    // Too broad, hides issues
}
```

### LINQ Usage

```csharp
// DO: Clear, method-style LINQ
var availableCars = await _carRepository.GetAllAsync();
var economyAvailable = availableCars
    .Where(c => c.Category == CarCategory.Economy && c.IsAvailable)
    .OrderBy(c => c.PricePerDay)
    .ToList();

// DON'T: Complex nested LINQ
var result = availableCars.Where(c => c.Category == CarCategory.Economy 
    && c.IsAvailable).OrderBy(c => c.PricePerDay).ThenBy(c => c.Model
    .Length).Where(c => c.IsAvailable).ToList();
```

## Debugging Tips

### Enable Detailed Logging
```csharp
Console.WriteLine($"[DEBUG] Loading cars from: {_filePath}");
Console.WriteLine($"[DEBUG] Found {_cars.Count} cars");
```

### Inspect Domain Objects
```csharp
var rental = await _rentalRepository.GetByIdAsync(rentalId);
Console.WriteLine($"Rental: {rental}");  // Calls ToString()
```

### Test Isolated Components
```csharp
// Test money independently
var money = new Money(100);
var doubled = money.Multiply(2);
Assert.Equal(200, doubled.Amount);
```

## Performance Considerations

### Current Limitations (JSON-based)
- All entities loaded into memory
- No query optimization
- File I/O on every operation

### Optimization Strategies (Future)
1. **Database Migration** - Replace JSON with SQL
2. **Caching** - Cache frequently accessed data
3. **Lazy Loading** - Load data on demand
4. **Indexing** - Speed up lookups
5. **Batching** - Reduce file writes

## Extending the System

### Adding a New Pricing Category
1. Add enum value
2. Create strategy class
3. Register in dictionary
4. Add tests
5. UI integration

### Adding Payment Methods
1. Create interface: `IPaymentProcessor`
2. Implement for PayPal, Stripe, etc.
3. Update RentalService
4. Add UI for payment selection

### Multi-Location Support
1. Add LocationId to repositories
2. Filter data by location
3. Update UI with location selection
4. Add location-specific pricing

## Troubleshooting

### "File not found" errors
- Check `data/` directory exists
- Verify JSON path is correct
- Run from project root

### "Entity not found" errors
- Ensure sample data initialized
- Check ID generation
- Verify repository queries

### "State transition invalid" errors
- Check rental status before operation
- Verify business rules enforced
- Review entity state machine

---

**Document Version**: 1.0
**Last Updated**: April 2026
**Audience**: Developers
