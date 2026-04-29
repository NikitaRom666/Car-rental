# System Vision & Project Goals

## Problem Statement

Managing a car rental business involves complex operations including tracking vehicle availability, managing reservations across multiple dates, calculating prices based on vehicle categories, processing payments, and maintaining customer records. Manual or poorly designed systems lead to:

- Double-booking of vehicles
- Incorrect pricing calculations
- Payment tracking errors
- Lost customer data
- Difficult to scale operations

## Solution Overview

The Car Rental System provides a comprehensive, technology-enabled solution with:

- **Automated availability tracking** preventing double-bookings
- **Intelligent pricing strategies** supporting multiple vehicle categories
- **Clean, maintainable architecture** enabling future enhancements
- **Reliable data persistence** with JSON-based file storage
- **Easy extensibility** through design patterns

## Key Stakeholders

1. **Rental Agency Managers** - Want to manage operations efficiently
2. **System Administrators** - Need reliable, maintainable code
3. **Developers** - Value clean architecture and design patterns
4. **Customers** - Want to quickly book and rent vehicles
5. **Accounting Team** - Require accurate payment tracking

## Project Goals

### Primary Goals (Must Have)

1. **Functional Rental System**
   - Users can view available cars
   - Users can create reservations
   - Users can start and complete rentals
   - System prevents booking conflicts

2. **Clean Architecture**
   - Clear separation of concerns
   - Domain layer independent of frameworks
   - Testable code structure
   - Dependency injection throughout

3. **Rich Domain Model**
   - Business logic encapsulated in entities
   - Validation at point of creation
   - State machines for complex objects
   - No anemic models

4. **SOLID Principles**
   - Single Responsibility
   - Open/Closed for extension
   - Liskov Substitution
   - Interface Segregation
   - Dependency Inversion

5. **Design Patterns**
   - Repository Pattern for data access
   - Strategy Pattern for pricing
   - Observer Pattern for events
   - Dependency Injection

### Secondary Goals (Should Have)

1. **Comprehensive Testing**
   - Unit tests for business logic
   - Integration tests for workflows
   - Negative test cases
   - Edge case handling

2. **Production-Quality Code**
   - XML documentation
   - Consistent naming conventions
   - Error handling
   - Graceful degradation

3. **Extensibility**
   - New payment methods easily added
   - Additional pricing strategies
   - Support for multiple locations
   - Plugin-style architecture

### Tertiary Goals (Nice to Have)

1. **Advanced Features**
   - Insurance add-on (Decorator pattern)
   - Email notifications
   - Advanced reporting
   - Performance analytics

2. **Developer Experience**
   - Comprehensive documentation
   - Example code and patterns
   - Clear project structure
   - Easy local setup

## Business Rules (Domain Constraints)

### Rental Rules
- Only available cars can be rented
- Rental dates must not conflict with existing bookings
- Minimum rental duration: 1 day
- Car becomes unavailable when rental starts
- Car becomes available when rental completes

### Reservation Rules
- Reservations are for future dates
- Cannot reserve same start and end date
- Reservations must check for conflicts
- Can cancel active reservations

### Pricing Rules
- Different rates per vehicle category
- **Economy**: Standard rate (no discount)
- **Business**: 5% discount for 7+ day rentals
- **Premium**: Progressive discounts (5% @ 4 days, 10% @ 8 days, 15% @ 15 days)

### Payment Rules
- All rentals require payment
- Payment lifecycle: Pending → Paid/Failed
- Only successful rentals generate charges
- Transaction reference tracks all payments

## Success Criteria

### Technical Success
- ✅ Application builds without warnings
- ✅ All unit tests pass
- ✅ Zero code duplication (DRY principle)
- ✅ Cyclomatic complexity < 10 per method
- ✅ Code coverage > 80%

### Functional Success
- ✅ Users can complete full rental workflow
- ✅ System prevents booking conflicts
- ✅ Pricing calculated correctly per strategy
- ✅ All business rules enforced
- ✅ Data persisted reliably

### Architectural Success
- ✅ Domain layer has no dependencies
- ✅ Clean separation of concerns
- ✅ All SOLID principles demonstrated
- ✅ Repository pattern fully implemented
- ✅ Dependency injection container properly configured

### Documentation Success
- ✅ README guides setup and usage
- ✅ Developer guide explains architecture
- ✅ Code has XML documentation
- ✅ Examples demonstrate patterns
- ✅ Test cases document expected behavior

## Scope Definition

### In Scope
- Core rental, reservation, and payment management
- Four-layer clean architecture
- JSON-based persistence
- Console UI for demonstration
- Comprehensive test suite
- Complete documentation

### Out of Scope
- Web UI or REST API
- Database layer (JSON only)
- Authentication/Authorization
- Email integration
- Advanced reporting
- Performance optimization
- Mobile application

## Evolution Path (Iterations)

### Iteration 1: Core Rental System
- Basic entities and value objects
- Simple repository with JSON storage
- Core rental start/completion workflow
- Manual testing

### Iteration 2: Pricing & Reservation
- Pricing strategy pattern
- Reservation system
- Availability checking
- Unit tests for pricing

### Iteration 3: Polish & Testing
- Full test coverage
- Error handling
- Event logging (Observer pattern)
- Integration tests

### Iteration 4: Documentation
- Architecture documentation
- User guide
- Developer guide
- Example flows

## Risk Management

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|-----------|
| Date conflicts not prevented | High | Medium | Early focus on DateRange value object |
| Pricing calculation errors | High | Low | Strategy pattern with comprehensive tests |
| Data loss | High | Low | Regular file backups, transaction logging |
| Complex state management | Medium | Medium | State machines in entities with tests |
| Scope creep | Medium | Medium | Clear scope definition, feature gating |

## Assumptions

1. Single location initially (multi-location deferred)
2. JSON storage sufficient for demonstration
3. Console UI adequate for prototype
4. .NET 6+ available in deployment
5. Basic business rules understood by team

## Dependencies

- .NET 6 SDK
- Visual Studio or VS Code
- xUnit for testing
- System.Text.Json (built-in)
- Microsoft.Extensions.DependencyInjection

## Glossary

- **Entity**: Domain object with identity and business logic
- **Value Object**: Immutable domain concept (Money, DateRange)
- **Repository**: Data access abstraction
- **Service**: Orchestrates use cases and business workflows
- **DTO**: Data Transfer Object for layer boundaries
- **Strategy**: Pluggable algorithm (pricing)
- **Observer**: Event notification pattern
- **Use Case**: Single business process (StartRental)

---

**Document Version**: 1.0
**Last Updated**: April 2026
**Prepared By**: Development Team
