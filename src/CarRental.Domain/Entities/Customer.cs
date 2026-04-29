namespace CarRental.Domain.Entities;

// Клієнт, що орендує чи резервує авто
public class Клієнт
{
    public Guid Id { get; private set; }
    public string Ім'я { get; private set; }
    public string Пошта { get; private set; }
    public string Телефон { get; private set; }
    public string НомерВодійськогоСвідоцтва { get; private set; }
    public DateTime ЧасСтворення { get; private set; }

    private Клієнт() { }

    public Клієнт(string ім'я, string пошта, string телефон, string номерВодійськогоСвідоцтва)
    {
        if (string.IsNullOrWhiteSpace(ім'я))
            throw new ArgumentException("Ім'я не може бути порожним", nameof(ім'я));
        if (string.IsNullOrWhiteSpace(пошта) || !пошта.Contains("@"))
            throw new ArgumentException("Потрібна коректна пошта", nameof(пошта));
        if (string.IsNullOrWhiteSpace(телефон))
            throw new ArgumentException("Телефон не може бути порожним", nameof(телефон));
        if (string.IsNullOrWhiteSpace(номерВодійськогоСвідоцтва))
            throw new ArgumentException("Номер свідоцтва не може бути порожним", nameof(номерВодійськогоСвідоцтва));

        Id = Guid.NewGuid();
        Ім'я = ім'я;
        Пошта = пошта;
        Телефон = телефон;
        НомерВодійськогоСвідоцтва = номерВодійськогоСвідоцтва;
        ЧасСтворення = DateTime.UtcNow;
    }

    public override string ToString() => $"{Ім'я} ({Пошта})";
}
