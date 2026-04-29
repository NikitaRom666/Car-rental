# Car Rental System - Complete Project Documentation

A production-grade Car Rental Management System built with C# and .NET 6, demonstrating Clean Architecture, SOLID principles, and advanced design patterns. This is a comprehensive academic capstone project designed to showcase enterprise-level software engineering practices.

## Project Overview

The Car Rental System is a full-featured application that manages vehicle rentals, reservations, customers, and payments. It demonstrates best practices in object-oriented design, domain-driven development, and layered architecture.

### Key Features

- **Rich Domain Models** with enforced business rules and encapsulation
- **Rental Management**: Create, start, complete, and cancel rentals
- **Reservation System**: Reserve vehicles for future dates with conflict detection
- **Pricing Strategies**: Dynamic pricing based on vehicle category
- **Payment Tracking**: Complete payment lifecycle management
- **JSON-based Persistence**: File-based data storage with JSON serialization
- **Dependency Injection**: Full DI/IoC container setup
- **Event Logging**: Observer pattern for system events
- **Comprehensive Testing**: Unit and integration tests with xUnit

## Architecture

This project follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────────────┐
│          Console UI Layer (User Interface)      │
├─────────────────────────────────────────────────┤
│     Application Layer (Use Cases & Services)    │
├─────────────────────────────────────────────────┤
│    Infrastructure Layer (Data Persistence)      │
├─────────────────────────────────────────────────┤
│    Domain Layer (Business Rules & Entities)     │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

**Domain Layer** (`CarRental.Domain`)
- Pure business logic and domain entities
- Rich entities with behavior and validation
- Value objects (Money, DateRange)
- Custom exceptions for business rules
- No external dependencies

**Application Layer** (`CarRental.Application`)
- Use case implementations
- Service interfaces and contracts
- Data Transfer Objects (DTOs)
- Strategy pattern implementations
- Orchestration of business workflows

**Infrastructure Layer** (`CarRental.Infrastructure`)
- Repository implementations
- JSON file persistence
- Dependency injection setup
- Framework-specific concerns

**Console UI Layer** (`CarRental.Console`)
- Menu-driven CLI interface
- User input/output handling
- Dependency injection configuration
- Sample data initialization

## SOLID Principles Implementation

### Single Responsibility Principle (SRP)
- Each class has one reason to change
- `Money` handles only monetary operations
- `DateRange` handles only date validation
- Repositories handle only data access
- Services handle only business logic

### Open/Closed Principle (OCP)
- Pricing strategies extensible without modification
- New payment methods can be added through inheritance
- Repository pattern allows new data storage mechanisms

### Liskov Substitution Principle (LSP)
- All pricing strategies implement `IPricingStrategy` interface
- All repositories implement their respective interfaces
- Substitutable without breaking functionality

### Interface Segregation Principle (ISP)
- Separate repository interfaces (`ICarRepository`, `IRentalRepository`, etc.)
- Services depend on specific interfaces, not large monolithic contracts

### Dependency Inversion Principle (DIP)
- High-level modules depend on abstractions, not implementations
- `RentalService` depends on repository interfaces
- DI container manages concrete implementations

## Design Patterns

### 1. Repository Pattern
Abstracts data access logic behind interfaces:
```csharp
public interface ICarRepository
{
    Task<Car?> GetByIdAsync(Guid id);
    Task<IEnumerable<Car>> GetAvailableCarsAsync();
    Task AddAsync(Car car);
    Task UpdateAsync(Car car);
}
```

### 2. Strategy Pattern (Mandatory)
Different pricing algorithms for car categories:
```csharp
public interface IPricingStrategy
{
    CarCategory Category { get; }
    Money CalculatePrice(Money dailyRate, int daysCount);
}

// Three implementations:
- EconomyPricingStrategy
- BusinessPricingStrategy
- PremiumPricingStrategy
```

### 3. Observer Pattern (Bonus)
Event notification system for rental lifecycle:
```csharp
public interface IRentalEventObserver
{
    Task OnRentalCreatedAsync(Guid rentalId, Guid customerId, Guid carId);
    Task OnRentalStartedAsync(Guid rentalId);
    Task OnRentalCompletedAsync(Guid rentalId);
    Task OnRentalCancelledAsync(Guid rentalId);
}
```

### 4. Dependency Injection
Service container manages object creation and lifetime:
```csharp
services.AddInfrastructureServices("data");
services.AddScoped<RentalService>(provider =>
    new RentalService(
        provider.GetRequiredService<ICarRepository>(),
        // ... other dependencies
    )
);
```

## Domain Model

### Core Entities

**Car** - Vehicle available for rental
- Manages availability status
- Enforces validation on creation
- Immutable except for availability

**Customer** - Person who rents vehicles
- Validates email format
- Stores driver license information
- Immutable after creation

**Rental** - Active rental transaction
- State machine for rental lifecycle (Created → Active → Completed/Cancelled)
- Encapsulates business rules for state transitions
- Enforces: cannot rent unavailable car, cannot finish inactive rental

**Reservation** - Advance booking of a vehicle
- Prevents date conflicts
- Can be cancelled by customer
- Requires future dates only

**Payment** - Financial transaction
- State machine: Pending → Paid/Failed
- Tracks transaction references
- Enforces business rules for state transitions

**Location** - Physical rental location
- Immutable after creation
- Validates address information

### Value Objects

**Money**
- Type-safe monetary values
- Currency-aware operations
- Prevents negative amounts
- Supports equality comparison

**DateRange**
- Date validation (start < end)
- Overlap detection for conflict checking
- Inclusive day count calculation

### Enums

- `CarCategory`: Economy, Business, Premium
- `RentalStatus`: Created, Active, Completed, Cancelled
- `PaymentStatus`: Pending, Paid, Failed

## Business Rules (Enforced in Domain)

1. **Availability Rules**
   - Car must be available to start rental
   - Unavailable cars cannot be rented
   - Car becomes unavailable when rental starts

2. **Date Rules**
   - Rental start date must be before end date
   - Minimum rental duration is 1 day
   - Cannot reserve past dates
   - Date ranges cannot be identical

3. **State Transition Rules**
   - Rental: Created → Active → Completed/Cancelled (only valid transitions)
   - Cannot finish non-active rental
   - Cannot start rental twice
   - Cannot cancel completed rental

4. **Conflict Detection**
   - Overlapping rentals prevented
   - Overlapping reservations prevented

5. **Payment Rules**
   - Payment amount must be positive
   - Can only mark pending payment as paid/failed
   - Transaction reference required for successful payment

## Installation & Setup

### Prerequisites
- .NET 6 SDK or higher
- C# 10 or higher
- 100MB disk space for data files

### Clone & Build
```bash
cd CarRental
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Application
```bash
cd src/CarRental.Console
dotnet run
```

## Project Structure

```
CarRental/
├── src/
│   ├── CarRental.Domain/           # Business entities and rules
│   │   ├── Entities/               # Car, Rental, Customer, etc.
│   │   ├── Enums/                  # CarCategory, RentalStatus, etc.
│   │   ├── Exceptions/             # Custom domain exceptions
│   │   └── ValueObjects/           # Money, DateRange
│   ├── CarRental.Application/      # Use cases and services
│   │   ├── DTOs/                   # Request/Response objects
│   │   ├── Interfaces/             # Service and repository contracts
│   │   └── Services/               # RentalService orchestration
│   ├── CarRental.Infrastructure/   # Data access and DI
│   │   └── Persistence/            # JSON repository implementations
│   └── CarRental.Console/          # User interface
│       └── Program.cs              # CLI and DI setup
├── tests/
│   └── CarRental.Tests/            # xUnit test suite
│       └── UnitTests.cs            # Domain, strategy, flow tests
├── data/                           # JSON data files
│   ├── cars.json
│   ├── customers.json
│   ├── rentals.json
│   ├── reservations.json
│   ├── payments.json
│   └── locations.json
└── docs/                           # Documentation
    ├── README.md
    ├── vision.md
    ├── backlog.md
    ├── TESTING.md
    ├── DEVELOPER_GUIDE.md
    ├── USER_GUIDE.md
    ├── FINAL_REPORT.md
    └── DEMO.md
```

## Key Technologies

- **Language**: C# 10
- **Framework**: .NET 6
- **Persistence**: JSON with System.Text.Json
- **Testing**: xUnit
- **DI Container**: Microsoft.Extensions.DependencyInjection

## Testing Strategy

- **Unit Tests**: Value object operations, pricing calculations, state transitions
- **Business Rule Tests**: Availability validation, conflict detection, date validation
- **Integration Tests**: Complete rental workflows
- **Negative Tests**: Error cases, invalid operations

Run with:
```bash
dotnet test --verbosity normal
```

## Future Enhancements

1. Database persistence (SQL Server, PostgreSQL)
2. REST API layer for mobile access
3. Advanced reporting and analytics
4. Insurance add-on decorator pattern
5. Multi-location support with inter-branch transfers
6. Loyalty program and discounts
7. Vehicle maintenance scheduling
8. Email notifications
9. Web dashboard for management

## Code Quality Standards

- Clean, readable code following C# conventions
- XML documentation for public APIs
- Comprehensive error handling
- LINQ for data querying
- Immutable by default designq

## Contributing

This is an academic project for demonstration purposes. For real-world use, consider:
- Adding database layer
- Implementing authentication/authorization
- Adding API layer
- Performance optimization
- Security hardening

## License

Educational use only.

---

**Last Updated**: April 2026
**Version**: 1.0.0
**Status**: Production Ready
