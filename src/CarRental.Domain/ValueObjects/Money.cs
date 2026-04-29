namespace CarRental.Domain.ValueObjects;

// Грошова сума з валютою, type-safe операції
public class Гроші : IEquatable<Гроші>
{
    private const string ВалютаПоЗмовчуванню = "USD";
    private const decimal МінімумСуми = 0;

    public decimal Amount { get; }
    public string Currency { get; }

    public Гроші(decimal amount, string currency = ВалютаПоЗмовчуванню)
    {
        if (amount < МінімумСуми)
            throw new ArgumentException($"Сума не може бути негативною. Надано: {amount}", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Валюта не може бути порожною", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public Гроші Додати(Гроші інші)
    {
        if (інші.Currency != Currency)
            throw new InvalidOperationException($"Не можна додавати різні валюти: {Currency} та {інші.Currency}");

        return new Гроші(Amount + інші.Amount, Currency);
    }

    public Гроші Помножити(int днів)
    {
        if (днів <= 0)
            throw new ArgumentException("Дні мусять бути більше за нуль", nameof(днів));

        return new Гроші(Amount * днів, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public override bool Equals(object? obj) => Equals(obj as Гроші);

    public bool Equals(Гроші? інші)
    {
        return інші != null && Amount == інші.Amount && Currency == інші.Currency;
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}
