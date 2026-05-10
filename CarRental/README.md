# Car Rental

Layered C# application for booking vehicles with date-conflict detection and JSON persistence.

**Run:**
```bash
dotnet run --project ConsoleUI/ConsoleUI.csproj
```

**Test:**
```bash
dotnet test
```

## Architecture

| Layer | Purpose |
|-------|---------|
| **Domain** | Entities (Car, Booking, Customer), ValueObjects (BookingPeriod, VehicleCategory), Interfaces (ICarRepository, IBookingRepository), Validators |
| **Application** | BookingService, BookingQueryService, DTOs, Strategy pattern for pricing (Economy/Standard/SUV) |
| **Infrastructure** | FileCarRepository, FileBookingRepository (JSON persistence with async I/O) |
| **ConsoleUI** | Menu-driven interface with error handling |
| **Tests** | Unit tests (xUnit [Fact]) + Integration tests (file I/O, state persistence) |

## Key Features

- **Date Overlap Detection** - `GetOverlappingAsync()` prevents double-bookings
- **Category-Based Pricing** - Strategy pattern: EconomyBookingPricingStrategy, StandardBookingPricingStrategy, SuvBookingPricingStrategy
- **Async Persistence** - LoadAsync/SaveAsync for file operations
- **Fault Tolerance** - Error handling for missing files, corrupted JSON, and I/O failures
- **Clean Code** - No XML comments, encapsulated business rules in Domain layer

## Docs

- [Vision](docs/vision.md) | [Backlog](docs/backlog.md) | [Architecture](docs/architecture-diagram.md)  
- [Iteration 1](docs/iteration-1.md) | [Iteration 2](docs/iteration-2.md) | [Iteration 3](docs/iteration-3.md)
