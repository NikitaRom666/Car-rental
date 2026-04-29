
namespace CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;

// Платіж для оренди
public class Платіж
{
    public Guid Id { get; private set; }
    public Guid ОрендаId { get; private set; }
    public Гроші Сума { get; private set; }
    public СтатусПлатежу Статус { get; private set; }
    public DateTime ЧасСтворення { get; private set; }
    public DateTime? ЧасСплати { get; private set; }
    public string? РеквізитТранзакції { get; private set; }

    private Платіж() { }

    public Платіж(Guid орендаId, Гроші сума)
    {
        if (орендаId == Guid.Empty)
            throw new ArgumentException("ID оренди не може бути порожним", nameof(орендаId));
        if (сума == null)
            throw new ArgumentNullException(nameof(сума));
        if (сума.Amount <= 0)
            throw new ArgumentException("Сума мусить бути більше за нуль", nameof(сума));

        Id = Guid.NewGuid();
        ОрендаId = орендаId;
        Сума = сума;
        Статус = СтатусПлатежу.Очікує;
        ЧасСтворення = DateTime.UtcNow;
    }

    // Позначаємо як сплачено
    public void ПозначитиСплачено(string реквізитТранзакції)
    {
        if (Статус != СтатусПлатежу.Очікує)
            throw new InvalidOperationException(
                $"Можна позначити як сплачено лише зі стану Очікує, поточно: {Статус}");

        if (string.IsNullOrWhiteSpace(реквізитТранзакції))
            throw new ArgumentException("Реквізит не може бути порожним", nameof(реквізитТранзакції));

        Статус = СтатусПлатежу.Сплачено;
        ЧасСплати = DateTime.UtcNow;
        РеквізитТранзакції = реквізитТранзакції;
    }

    // Позначаємо як помилка
    public void ПозначитиПомилку()
    {
        if (Статус != СтатусПлатежу.Очікує)
            throw new InvalidOperationException(
                $"Можна позначити як помилка лише зі стану Очікує, поточно: {Статус}");

        Статус = СтатусПлатежу.Помилка;
    }

    public override string ToString() => 
        $"Платіж {Id}: {Сума} для оренди {ОрендаId}, статус: {Статус}";
}
