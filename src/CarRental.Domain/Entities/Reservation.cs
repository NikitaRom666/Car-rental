namespace CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;

// Резервування авто на період
public class Резервування
{
    public Guid Id { get; private set; }
    public Guid АвтоId { get; private set; }
    public Guid КлієнтId { get; private set; }
    public ПеріодДат ПеріодРезервування { get; private set; }
    public bool ЩеА́ктивне { get; private set; }
    public DateTime ЧасСтворення { get; private set; }
    public DateTime? ЧасСкасування { get; private set; }

    private Резервування() { }

    public Резервування(Guid автоId, Guid клієнтId, ПеріодДат періодРезервування)
    {
        if (автоId == Guid.Empty)
            throw new ArgumentException("ID авто не може бути порожним", nameof(автоId));
        if (клієнтId == Guid.Empty)
            throw new ArgumentException("ID клієнта не може бути порожним", nameof(клієнтId));
        if (періодРезервування == null)
            throw new ArgumentNullException(nameof(періодРезервування));

        // Перевіряємо майбутні дати
        if (періодРезервування.ДатаПочатку <= DateTime.UtcNow.Date)
            throw new InvalidOperationException("Резервування тільки на майбутні дати");

        Id = Guid.NewGuid();
        АвтоId = автоId;
        КлієнтId = клієнтId;
        ПеріодРезервування = періодРезервування;
        ЩеА́ктивне = true;
        ЧасСтворення = DateTime.UtcNow;
    }

    // Скасовуємо резервування
    public void Скасувати()
    {
        if (!ЩеА́ктивне)
            throw new InvalidOperationException("Не можна скасувати вже скасоване резервування");

        ЩеА́ктивне = false;
        ЧасСкасування = DateTime.UtcNow;
    }

    public override string ToString() => 
        $"Резервування {Id}: Авто {АвтоId}, Клієнт {КлієнтId}, {ПеріодРезервування}";
}
