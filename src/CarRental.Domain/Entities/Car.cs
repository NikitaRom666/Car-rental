namespace CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.ValueObjects;

// Авто для оренди з перевіркою доступності
public class Авто
{
    public Guid Id { get; private set; }
    public string Модель { get; private set; }
    public КатегоріяАвто Категорія { get; private set; }
    public Гроші ЦінаЗаДень { get; private set; }
    public bool ДоступНа { get; private set; }
    public Guid МісцеположенняId { get; private set; }
    public string НомерБереза { get; private set; }
    public int РікВипуску { get; private set; }
    public DateTime ЧасСтворення { get; private set; }

    private Авто() { }

    public Авто(string модель, КатегоріяАвто категорія, Гроші цінаЗаДень, Guid місцеположенняId, 
               string номерБереза, int рікВипуску)
    {
        if (string.IsNullOrWhiteSpace(модель))
            throw new ArgumentException("Модель не може бути порожною", nameof(модель));
        if (цінаЗаДень == null)
            throw new ArgumentNullException(nameof(цінаЗаДень));
        if (string.IsNullOrWhiteSpace(номерБереза))
            throw new ArgumentException("Номер не може бути порожним", nameof(номерБереза));
        if (місцеположенняId == Guid.Empty)
            throw new ArgumentException("ID місця не може бути порожним", nameof(місцеположенняId));
        if (рікВипуску < 1900 || рікВипуску > DateTime.UtcNow.Year)
            throw new ArgumentException($"Рік має бути між 1900 і {DateTime.UtcNow.Year}", 
                nameof(рікВипуску));

        Id = Guid.NewGuid();
        Модель = модель;
        Категорія = категорія;
        ЦінаЗаДень = цінаЗаДень;
        МісцеположенняId = місцеположенняId;
        НомерБереза = номерБереза;
        РікВипуску = рікВипуску;
        ДоступНа = true;
        ЧасСтворення = DateTime.UtcNow;
    }

    // Робимо недоступною
    public void РобитиНедоступною()
    {
        ДоступНа = false;
    }

    // Робимо доступною
    public void РобитиДоступною()
    {
        ДоступНа = true;
    }

    // Перевіряємо чи можна орендувати
    public void ПеревіритиДоступність()
    {
        if (!ДоступНа)
            throw new InvalidOperationException($"Авто '{Модель}' (Номер: {НомерБереза}) недоступна");
    }

    public override string ToString() => $"{Модель} ({Категорія}) - Номер: {НомерБереза}";
}
