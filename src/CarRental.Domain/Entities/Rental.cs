namespace CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;

// Оренда з перевіркою переходів статусу
public class Оренда
{
    public Guid Id { get; private set; }
    public Guid АвтоId { get; private set; }
    public Guid КлієнтId { get; private set; }
    public ПеріодДат ОрендериодД { get; private set; }
    public Гроші ЗагальнаЦіна { get; private set; }
    public СтатусОренди Статус { get; private set; }
    public DateTime ЧасСтворення { get; private set; }
    public DateTime? ЧасЗавершення { get; private set; }

    private Оренда() { }

    public Оренда(Guid автоId, Guid клієнтId, ПеріодДат орендариодД, Гроші загальнаЦіна)
    {
        if (автоId == Guid.Empty)
            throw new ArgumentException("ID авто не може бути порожним", nameof(автоId));
        if (клієнтId == Guid.Empty)
            throw new ArgumentException("ID клієнта не може бути порожним", nameof(клієнтId));
        if (орендариодД == null)
            throw new ArgumentNullException(nameof(орендариодД));
        if (загальнаЦіна == null)
            throw new ArgumentNullException(nameof(загальнаЦіна));

        Id = Guid.NewGuid();
        АвтоId = автоId;
        КлієнтId = клієнтId;
        ОрендериодД = орендариодД;
        ЗагальнаЦіна = загальнаЦіна;
        Статус = СтатусОренди.Створена;
        ЧасСтворення = DateTime.UtcNow;
    }

    // Активуємо оренду
    public void Активувати()
    {
        if (Статус != СтатусОренди.Створена)
            throw new InvalidOperationException(
                $"Можна активувати лише зі стану Створена, поточно: {Статус}");

        Статус = СтатусОренди.Активна;
    }

    // Завершуємо оренду
    public void Завершити()
    {
        if (Статус != СтатусОренди.Активна)
            throw new InvalidOperationException(
                $"Можна завершити лише зі стану Активна, поточно: {Статус}");

        Статус = СтатусОренди.Завершена;
        ЧасЗавершення = DateTime.UtcNow;
    }

    // Скасовуємо оренду
    public void Скасувати()
    {
        if (Статус == СтатусОренди.Завершена || Статус == СтатусОренди.Скасована)
            throw new InvalidOperationException(
                $"Не можна скасувати оренду зі стану {Статус}");

        Статус = СтатусОренди.Скасована;
    }

    public override string ToString() => 
        $"Оренда {Id}: Авто {АвтоId}, Клієнт {КлієнтId}, {ОрендериодД.КількістьДнів} днів, {ЗагальнаЦіна}";
}
