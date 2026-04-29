namespace CarRental.Domain.ValueObjects;

// Період між двома датами з перевіркою коректності
public class ПеріодДат : IEquatable<ПеріодДат>
{
    public DateTime ДатаПочатку { get; }
    public DateTime ДатаЗавершення { get; }

    public int КількістьДнів => (ДатаЗавершення.Date - ДатаПочатку.Date).Days + 1;

    public ПеріодДат(DateTime датаПочатку, DateTime датаЗавершення)
    {
        if (датаПочатку.Date > датаЗавершення.Date)
            throw new ArgumentException(
                $"Початок не може бути після завершення. Початок: {датаПочатку:yyyy-MM-dd}, Завершення: {датаЗавершення:yyyy-MM-dd}",
                nameof(датаПочатку));

        if (датаПочатку.Date == датаЗавершення.Date)
            throw new ArgumentException("Дати не можуть співпадати. Мінімум 1 день", nameof(датаЗавершення));

        ДатаПочатку = датаПочатку.Date;
        ДатаЗавершення = датаЗавершення.Date;
    }

    public bool ПересікаєтьсяЗ(ПеріодДат інший)
    {
        return ДатаПочатку <= інший.ДатаЗавершення && ДатаЗавершення >= інший.ДатаПочатку;
    }

    public bool Містить(DateTime дата)
    {
        return дата.Date >= ДатаПочатку && дата.Date <= ДатаЗавершення;
    }

    public override string ToString() => $"{ДатаПочатку:yyyy-MM-dd} по {ДатаЗавершення:yyyy-MM-dd}";

    public override bool Equals(object? obj) => Equals(obj as ПеріодДат);

    public bool Equals(ПеріодДат? інший)
    {
        return інший != null && ДатаПочатку == інший.ДатаПочатку && ДатаЗавершення == інший.ДатаЗавершення;
    }

    public override int GetHashCode() => HashCode.Combine(ДатаПочатку, ДатаЗавершення);
}
