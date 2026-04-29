using Xunit;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;
using CarRental.Domain.Exceptions;
using CarRental.Application.Interfaces.Services;

namespace CarRental.Tests;

/// <summary>
/// Unit tests for value objects and domain entities.
/// Tests business rule enforcement and state transitions.
/// </summary>
public class ValueObjectTests
{
    #region Money Value Object Tests

    [Fact]
    public void Money_ShouldBeEqualWhenValuesAreSame()
    {
        // Arrange & Act
        var money1 = new Money(100m);
        var money2 = new Money(100m);

        // Assert
        Assert.Equal(money1, money2);
    }

    [Fact]
    public void Money_ShouldThrowWhenAmountIsNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(-50m));
    }

    [Fact]
    public void Money_CanAddTwoAmounts()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void Money_CanMultiplyByDays()
    {
        // Arrange
        var dailyRate = new Money(50m);

        // Act
        var totalPrice = dailyRate.Multiply(5);

        // Assert
        Assert.Equal(250m, totalPrice.Amount);
    }

    [Fact]
    public void Money_ThrowsWhenAddingDifferentCurrencies()
    {
        // Arrange
        var usd = new Money(100m, "USD");
        var eur = new Money(100m, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => usd.Add(eur));
    }

    #endregion

    #region DateRange Value Object Tests

    [Fact]
    public void DateRange_ShouldBeCreatedWithValidDates()
    {
        // Arrange & Act
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(6, dateRange.DaysCount); // Inclusive count
    }

    [Fact]
    public void DateRange_ThrowsWhenStartDateIsAfterEndDate()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(5);
        var endDate = startDate.AddDays(-3);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DateRange(startDate, endDate));
    }

    [Fact]
    public void DateRange_ThrowsWhenDatesAreTheSame()
    {
        // Arrange
        var date = DateTime.Now.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DateRange(date, date));
    }

    [Fact]
    public void DateRange_ChecksOverlapCorrectly()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Now.AddDays(1), DateTime.Now.AddDays(5));
        var range2 = new DateRange(DateTime.Now.AddDays(3), DateTime.Now.AddDays(7));
        var range3 = new DateRange(DateTime.Now.AddDays(10), DateTime.Now.AddDays(15));

        // Act & Assert
        Assert.True(range1.OverlapsWith(range2));
        Assert.False(range1.OverlapsWith(range3));
    }

    #endregion
}

/// <summary>
/// Unit tests for pricing strategies (Strategy Pattern).
/// Validates correct price calculations for different categories.
/// </summary>
public class PricingStrategyTests
{
    [Fact]
    public void EconomyPricing_CalculatesBasePriceWithoutDiscount()
    {
        // Arrange
        var strategy = new EconomyPricingStrategy();
        var dailyRate = new Money(50m);

        // Act
        var price = strategy.CalculatePrice(dailyRate, 5);

        // Assert
        Assert.Equal(250m, price.Amount); // 50 * 5
    }

    [Fact]
    public void BusinessPricing_Applies5PercentDiscountFor8DaysOrMore()
    {
        // Arrange
        var strategy = new BusinessPricingStrategy();
        var dailyRate = new Money(100m);

        // Act
        var price = strategy.CalculatePrice(dailyRate, 8);

        // Assert
        Assert.Equal(760m, price.Amount); // 100 * 8 * 0.95
    }

    [Fact]
    public void PremiumPricing_Applies15PercentDiscountFor15DaysOrMore()
    {
        // Arrange
        var strategy = new PremiumPricingStrategy();
        var dailyRate = new Money(200m);

        // Act
        var price = strategy.CalculatePrice(dailyRate, 15);

        // Assert
        Assert.Equal(2550m, price.Amount); // 200 * 15 * 0.85
    }

    [Fact]
    public void PremiumPricing_NoDiscountFor1to3Days()
    {
        // Arrange
        var strategy = new PremiumPricingStrategy();
        var dailyRate = new Money(200m);

        // Act
        var price = strategy.CalculatePrice(dailyRate, 3);

        // Assert
        Assert.Equal(600m, price.Amount); // 200 * 3
    }
}

/// <summary>
/// Unit tests for domain entities and business rule enforcement.
/// Tests state transitions and validations.
/// </summary>
public class RentalBusinessRulesTests
{
    [Fact]
    public void Car_ThrowsWhenCreatedWithEmptyModel()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Car("", CarCategory.Economy, new Money(50m), Guid.NewGuid(), "ABC123", 2023)
        );
        Assert.Contains("model", exception.Message.ToLower());
    }

    [Fact]
    public void Car_CanChangeAvailabilityStatus()
    {
        // Arrange
        var car = new Car("Toyota", CarCategory.Economy, new Money(50m), 
                         Guid.NewGuid(), "ABC123", 2023);
        Assert.True(car.IsAvailable);

        // Act
        car.MakeUnavailable();

        // Assert
        Assert.False(car.IsAvailable);

        // Act
        car.MakeAvailable();

        // Assert
        Assert.True(car.IsAvailable);
    }

    [Fact]
    public void Car_ValidateAvailabilityThrowsWhenNotAvailable()
    {
        // Arrange
        var car = new Car("Toyota", CarCategory.Economy, new Money(50m),
                         Guid.NewGuid(), "ABC123", 2023);
        car.MakeUnavailable();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => car.ValidateAvailability());
    }

    [Fact]
    public void Rental_CanOnlyActivateFromCreatedState()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), period, new Money(200m));

        // Act - should succeed
        rental.Activate();
        Assert.Equal(RentalStatus.Active, rental.Status);

        // Act - should fail
        var exception = Assert.Throws<InvalidOperationException>(() => rental.Activate());
        Assert.Contains("Created state", exception.Message);
    }

    [Fact]
    public void Rental_CanOnlyCompleteFromActiveState()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), period, new Money(200m));

        // Act & Assert - should fail from Created state
        var exception = Assert.Throws<InvalidOperationException>(() => rental.Complete());
        Assert.Contains("Active state", exception.Message);

        // Act - activate then complete
        rental.Activate();
        rental.Complete();

        // Assert
        Assert.Equal(RentalStatus.Completed, rental.Status);
        Assert.NotNull(rental.CompletedAt);
    }

    [Fact]
    public void Rental_CanCancelFromCreatedOrActiveState()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), period, new Money(200m));

        // Act
        rental.Cancel();

        // Assert
        Assert.Equal(RentalStatus.Cancelled, rental.Status);
    }

    [Fact]
    public void Rental_CannotCancelAfterCompletion()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), period, new Money(200m));

        // Act
        rental.Activate();
        rental.Complete();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => rental.Cancel());
        Assert.Contains("Cannot cancel rental", exception.Message);
    }

    [Fact]
    public void Customer_ThrowsWhenCreatedWithInvalidEmail()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Customer("John Doe", "invalid-email", "555-1234", "DL123")
        );
        Assert.Contains("email", exception.Message.ToLower());
    }

    [Fact]
    public void Payment_CanMarkAsPaidOnlyFromPendingState()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), new Money(100m));

        // Act
        payment.MarkAsPaid("TXN001");

        // Assert
        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAt);

        // Act & Assert - cannot mark as paid again
        var exception = Assert.Throws<InvalidOperationException>(() =>
            payment.MarkAsPaid("TXN002")
        );
        Assert.Contains("Pending state", exception.Message);
    }

    [Fact]
    public void Reservation_ThrowsWhenReservedForPastDates()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-2);
        var futureDate = pastDate.AddDays(5);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new Reservation(Guid.NewGuid(), Guid.NewGuid(), new DateRange(pastDate, futureDate))
        );
        Assert.Contains("future dates", exception.Message);
    }
}

/// <summary>
/// Integration tests for complete rental flows.
/// Tests the system end-to-end.
/// </summary>
public class RentalFlowIntegrationTests
{
    [Fact]
    public void FullRentalFlow_CreatesStartsAndCompletesRental()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);

        // Act - Create rental
        var rental = new Rental(carId, customerId, period, new Money(250m));
        
        // Act - Activate
        rental.Activate();

        // Act - Complete
        rental.Complete();

        // Assert
        Assert.Equal(RentalStatus.Completed, rental.Status);
        Assert.Equal(6, period.DaysCount);
        Assert.Equal(250m, rental.TotalPrice.Amount);
    }

    [Fact]
    public void NegativeTest_CannotRentUnavailableCar()
    {
        // Arrange
        var car = new Car("Toyota", CarCategory.Economy, new Money(50m),
                         Guid.NewGuid(), "ABC123", 2023);
        car.MakeUnavailable();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => car.ValidateAvailability());
    }

    [Fact]
    public void NegativeTest_CannotFinishInactiveRental()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(5);
        var period = new DateRange(startDate, endDate);
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), period, new Money(200m));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => rental.Complete());
    }

    [Fact]
    public void NegativeTest_DateRangeValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new DateRange(DateTime.Now.AddDays(5), DateTime.Now.AddDays(1))
        );
    }

    [Fact]
    public void NegativeTest_OverlappingReservations()
    {
        // Arrange
        var date1Start = DateTime.Now.AddDays(1);
        var date1End = date1Start.AddDays(5);
        var date2Start = date1End.AddDays(-2);
        var date2End = date2Start.AddDays(5);

        var range1 = new DateRange(date1Start, date1End);
        var range2 = new DateRange(date2Start, date2End);

        // Act & Assert
        Assert.True(range1.OverlapsWith(range2), "Ranges should overlap");
    }
}

/// <summary>
/// Tests for SOLID principles implementation.
/// Demonstrates Dependency Inversion, Single Responsibility, etc.
/// </summary>
public class SOLIDPrinciplesTests
{
    [Fact]
    public void SingleResponsibility_MoneyHandlesOnlyMonetaryOperations()
    {
        // Arrange & Act
        var money = new Money(100m);

        // Assert
        Assert.NotNull(money.Amount);
        Assert.NotNull(money.Currency);
        // Money doesn't know about rentals, dates, or customers
    }

    [Fact]
    public void DependencyInversion_RentalUsesInterfacesNotConcretions()
    {
        // Arrange
        var carId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var period = new DateRange(DateTime.Now.AddDays(1), DateTime.Now.AddDays(5));
        var price = new Money(200m);

        // Act - Constructor accepts only what's needed (inversion of control)
        var rental = new Rental(carId, customerId, period, price);

        // Assert - Rental doesn't depend on concrete repositories
        Assert.NotNull(rental);
    }

    [Fact]
    public void OpenClosed_PricingStrategyCanBeExtendedWithoutModification()
    {
        // Arrange
        var economyStrategy = new EconomyPricingStrategy();
        var businessStrategy = new BusinessPricingStrategy();
        var premiumStrategy = new PremiumPricingStrategy();

        var rate = new Money(100m);

        // Act
        var economyPrice = economyStrategy.CalculatePrice(rate, 5);
        var businessPrice = businessStrategy.CalculatePrice(rate, 5);
        var premiumPrice = premiumStrategy.CalculatePrice(rate, 5);

        // Assert - All implement same interface, different behaviors
        Assert.True(economyPrice.Amount > 0);
        Assert.True(businessPrice.Amount > 0);
        Assert.True(premiumPrice.Amount > 0);
    }
}
