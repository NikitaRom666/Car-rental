namespace CarRental.Domain.Entities;

// Місцеположення для орендування
public class Місцеположення
{
    public Guid Id { get; private set; }
    public string Назва { get; private set; }
    public string Адреса { get; private set; }
    public string Місто { get; private set; }
    public string ПоштовийКод { get; private set; }
    public DateTime ЧасСтворення { get; private set; }

    private Місцеположення() { }

    public Місцеположення(string назва, string адреса, string місто, string поштовийКод)
    {
        if (string.IsNullOrWhiteSpace(назва))
            throw new ArgumentException("Назва не може бути порожною", nameof(назва));
        if (string.IsNullOrWhiteSpace(адреса))
            throw new ArgumentException("Адреса не може бути порожною", nameof(адреса));
        if (string.IsNullOrWhiteSpace(місто))
            throw new ArgumentException("Місто не може бути порожним", nameof(місто));
        if (string.IsNullOrWhiteSpace(поштовийКод))
            throw new ArgumentException("Поштовий код не може бути порожним", nameof(поштовийКод));

        Id = Guid.NewGuid();
        Назва = назва;
        Адреса = адреса;
        Місто = місто;
        ПоштовийКод = поштовийКод;
        ЧасСтворення = DateTime.UtcNow;
    }
}
