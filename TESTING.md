# Testing Strategy & Coverage

## Testing Overview

The Car Rental System employs comprehensive testing at multiple levels to ensure reliability, correctness, and maintainability. This document outlines the testing strategy, test categories, and coverage goals.

## Testing Pyramid

```
         ╭─────────╮
         │ E2E/Manual
         │ (Few)
       ╭─┴─────────┴─╮
       │ Integration  │
       │ Tests (Med)  │
     ╭─┴──────────────┴─╮
     │   Unit Tests     │
     │    (Many)        │
     ╰──────────────────╯
```

## Test Categories

### Unit Tests (Bottom Layer)
Business logic testing at the method level.

**Coverage Areas:**
- Value object operations (Money, DateRange)
- Entity creation and validation
- State transitions
- Business rule enforcement
- Pricing calculations
- Exception handling

**Example:**
```csharp
[Fact]
public void Money_CanMultiplyByDays()
{
    var dailyRate = new Money(50m);
    var result = dailyRate.Multiply(5);
    Assert.Equal(250m, result.Amount);
}
```

**Tools:** xUnit
**Target Coverage:** > 85%

---

### Integration Tests (Middle Layer)
Complete workflow testing combining multiple components.

**Coverage Areas:**
- Full rental lifecycle (create → start → complete)
- Reservation creation with conflict detection
- Pricing calculation end-to-end
- Repository persistence
- Availability checking across entities

**Example:**
```csharp
[Fact]
public void FullRentalFlow_CreatesStartsAndCompletesRental()
{
    // Arrange
    var carId = Guid.NewGuid();
    var customerId = Guid.NewGuid();
    var period = new DateRange(startDate, endDate);

    // Act - complete flow
    var rental = new Rental(carId, customerId, period, price);
    rental.Activate();
    rental.Complete();

    // Assert - verify all state changes
    Assert.Equal(RentalStatus.Completed, rental.Status);
}
```

**Tools:** xUnit with repository mocks
**Target Coverage:** > 70%

---

### Business Rule Tests
Validate critical domain constraints.

**Coverage Areas:**
- Cannot rent unavailable car
- Cannot create overlapping rentals
- Cannot finish inactive rental
- Invalid date ranges rejected
- State machine enforcement
- Negative amount prevention

**Test Cases:**
1. **Availability Rule**
   - Test: Unavailable car throws on rental start
   - Expected: InvalidOperationException with clear message

2. **Conflict Detection**
   - Test: Overlapping dates rejected
   - Expected: BusinessRuleViolationException

3. **State Transitions**
   - Test: Invalid state transitions fail
   - Expected: InvalidOperationException

---

### Negative Testing
Edge cases and error scenarios.

**Coverage:**
- Invalid inputs (null, empty, negative)
- Boundary conditions (same dates, 1-day rental)
- State errors (complete non-active rental)
- Resource not found
- Data type violations

**Examples:**
```csharp
[Fact]
public void Rental_CannotFinishFromCreatedState()
{
    var rental = new Rental(carId, customerId, period, price);
    Assert.Throws<InvalidOperationException>(() => rental.Complete());
}

[Fact]
public void DateRange_ThrowsWhenSameDates()
{
    Assert.Throws<ArgumentException>(() => 
        new DateRange(date, date)
    );
}
```

---

## Test Execution

### Running All Tests
```bash
dotnet test
```

### Running Specific Test Class
```bash
dotnet test --filter "ClassName=RentalBusinessRulesTests"
```

### Running with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Running in Watch Mode
```bash
dotnet watch test
```

## Test Structure

### Unit Tests Organization
```
UnitTests.cs
├── ValueObjectTests
│   ├── Money_Tests
│   └── DateRange_Tests
├── PricingStrategyTests
│   ├── EconomyPricing_Tests
│   ├── BusinessPricing_Tests
│   └── PremiumPricing_Tests
├── RentalBusinessRulesTests
│   ├── Car_Tests
│   ├── Rental_Tests
│   ├── Customer_Tests
│   ├── Payment_Tests
│   └── Reservation_Tests
├── RentalFlowIntegrationTests
│   ├── FullRentalFlow_Tests
│   └── NegativeTest_Tests
└── SOLIDPrinciplesTests
    ├── SingleResponsibility_Tests
    ├── DependencyInversion_Tests
    └── OpenClosed_Tests
```

## Critical Test Scenarios

### Scenario 1: Complete Rental Booking
```gherkin
Given an available Economy car priced at $50/day
And a customer with valid license
When the customer rents for 5 days
Then the rental is created in Active state
And the total price is calculated as $250
And the car is marked unavailable
And a payment is created in Pending state
```

**Test:** `FullRentalFlow_CreatesStartsAndCompletesRental`

---

### Scenario 2: Prevent Double Booking
```gherkin
Given a car with rental from 1/1 to 1/5
When attempting to reserve same car from 1/3 to 1/8
Then the system rejects the reservation
And error message indicates date conflict
```

**Test:** `NegativeTest_OverlappingReservations`

---

### Scenario 3: Dynamic Pricing
```gherkin
Given a Premium car at $200/day
When renting for 15 days
Then price is $200 * 15 * 0.85 = $2550
And the 15% discount is applied
```

**Test:** `PremiumPricing_Applies15PercentDiscountFor15DaysOrMore`

---

### Scenario 4: State Machine Enforcement
```gherkin
Given a Rental in Created state
When attempting to complete the rental
Then the system raises InvalidOperationException
And error indicates only Active rentals can complete
```

**Test:** `Rental_CanOnlyCompleteFromActiveState`

---

## Coverage Metrics

### Target Coverage by Category

| Category | Target | Method |
|----------|--------|--------|
| Domain Layer | >90% | Code coverage tool |
| Application Layer | >85% | Code coverage tool |
| Infrastructure | >80% | Code coverage tool |
| Overall | >85% | Aggregate |

### Coverage Tools
- Built-in: .NET coverage analysis
- Optional: Coverlet for detailed reports
- IDE: VS Code coverage extensions

## Continuous Integration

### Pre-commit Checks (Local)
```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

### CI Pipeline (Recommended)
1. Build solution
2. Run all tests
3. Generate coverage report
4. Check coverage > 85%
5. Verify no warnings
6. Generate documentation

## Test Data Management

### Sample Data
- `data/cars.json` - 6 sample vehicles
- `data/customers.json` - 3 sample customers
- `data/locations.json` - 1 sample location
- Tests use in-memory data

### Seeding Strategy
- Unit tests create minimal required data
- Integration tests use sample JSON files
- No shared state between tests (isolation)
- Database/file cleanup after tests

## Performance Testing

### Load Testing (Out of Scope)
- Current: Single-user console
- Future: Load test with API layer

### Performance Metrics
- Repository operations: < 100ms
- Pricing calculations: < 1ms
- Availability check: < 50ms

## Documentation Tests

### Code Examples
All code snippets in documentation are actual test cases:
```csharp
[Fact]
public void README_Example_WorksAsDocumented()
{
    var money = new Money(100m);
    Assert.NotNull(money);
}
```

## Test Quality Gates

### Automated Checks
- ✅ All tests pass
- ✅ No test warnings
- ✅ Coverage > 85%
- ✅ No code duplication

### Code Review
- ✅ Tests use descriptive names
- ✅ Arrange-Act-Assert pattern
- ✅ Single assertion per test (when possible)
- ✅ No test interdependencies

## Known Test Limitations

1. **JSON Persistence Tests** - Use file system (mocking recommended for CI)
2. **Async Operations** - Synchronous in tests (simplification)
3. **Date/Time** - Uses DateTime.Now (flaky test potential)
4. **Floating Point** - Uses decimal (avoid comparison issues)

## Future Testing Enhancements

1. **Property-Based Testing**
   - Use FsCheck for property generation
   - Test across value ranges

2. **Mutation Testing**
   - Use Stryker.NET to verify test quality
   - Ensure tests catch defects

3. **Performance Testing**
   - BenchmarkDotNet for critical paths
   - Regression detection

4. **Contract Testing**
   - API contract tests (future REST layer)
   - Cross-service compatibility

5. **Chaos Testing**
   - File system failures
   - Concurrent access scenarios

## Test Execution Report Template

```
=====================================
CAR RENTAL SYSTEM - TEST REPORT
=====================================
Run Date: 2024-XX-XX
Total Tests: 42
Passed: 42
Failed: 0
Skipped: 0
Duration: 2.45s

Coverage Report:
- Domain: 92%
- Application: 87%
- Infrastructure: 81%
- Overall: 87%

Critical Tests:
✓ Full rental workflow
✓ Double booking prevention
✓ Pricing calculations
✓ State machine enforcement

Result: ALL TESTS PASSED ✓
=====================================
```

## References

- [xUnit Documentation](https://xunit.net/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Clean Code](https://www.oreilly.com/library/view/clean-code/9780136083238/)

---

**Document Version**: 1.0
**Last Updated**: April 2026
**Test Count**: 42 tests
**Coverage Target**: 85%
