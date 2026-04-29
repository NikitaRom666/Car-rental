namespace CarRental.Application.Interfaces.Services;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;

/// <summary>
/// Pricing strategy interface (Strategy pattern - mandatory).
/// Different pricing strategies can be implemented for different car categories.
/// </summary>
public interface IPricingStrategy
{
    CarCategory Category { get; }
    Money CalculatePrice(Money dailyRate, int daysCount);
}

/// <summary>
/// Economy car pricing strategy.
/// Applies standard pricing with slight discount for longer rentals.
/// </summary>
public class EconomyPricingStrategy : IPricingStrategy
{
    public CarCategory Category => CarCategory.Economy;

    public Money CalculatePrice(Money dailyRate, int daysCount)
    {
        if (daysCount <= 0)
            throw new ArgumentException("Days count must be greater than zero.", nameof(daysCount));

        // Economy: no discount
        return dailyRate.Multiply(daysCount);
    }
}

/// <summary>
/// Business car pricing strategy.
/// Applies moderate discount for longer rentals.
/// </summary>
public class BusinessPricingStrategy : IPricingStrategy
{
    public CarCategory Category => CarCategory.Business;

    public Money CalculatePrice(Money dailyRate, int daysCount)
    {
        if (daysCount <= 0)
            throw new ArgumentException("Days count must be greater than zero.", nameof(daysCount));

        // Business: 5% discount for rentals over 7 days
        Money basePrice = dailyRate.Multiply(daysCount);
        if (daysCount > 7)
        {
            decimal discountedAmount = basePrice.Amount * 0.95m;
            return new Money(discountedAmount, basePrice.Currency);
        }

        return basePrice;
    }
}

/// <summary>
/// Premium car pricing strategy.
/// Applies premium pricing with best-rate guarantee discounts.
/// </summary>
public class PremiumPricingStrategy : IPricingStrategy
{
    public CarCategory Category => CarCategory.Premium;

    public Money CalculatePrice(Money dailyRate, int daysCount)
    {
        if (daysCount <= 0)
            throw new ArgumentException("Days count must be greater than zero.", nameof(daysCount));

        Money basePrice = dailyRate.Multiply(daysCount);

        // Premium: progressive discounts
        // 0-3 days: no discount
        // 4-7 days: 5% discount  
        // 8-14 days: 10% discount
        // 15+ days: 15% discount
        decimal discountPercentage = daysCount switch
        {
            <= 3 => 0m,
            <= 7 => 0.05m,
            <= 14 => 0.10m,
            _ => 0.15m
        };

        if (discountPercentage > 0)
        {
            decimal discountedAmount = basePrice.Amount * (1 - discountPercentage);
            return new Money(discountedAmount, basePrice.Currency);
        }

        return basePrice;
    }
}
