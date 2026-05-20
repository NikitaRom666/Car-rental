# Посібник розробника (Developer Guide)

## Архітектурний дизайн проєкту
Проєкт реалізовано за тришаровою архітектурною моделлю:
* **Domain Layer:** Сутності `Car`, `Booking`, `Customer`, інтерфейси стратегій розрахунку цін.
* **Infrastructure Layer:** Репозиторій `JsonCarRepository`, що реалізує інтерфейс `ICarRepository`.
* **Application/UI Layer:** Консольний або графічний інтерфейс для взаємодії з користувачем.

## Розширення тарифної сітки (Паттерн Strategy)
Для додавання нового типу розрахунку цін (наприклад, святковий тариф) достатньо створити клас, що реалізує інтерфейс `ITariffStrategy`:
```csharp
public class HolidayTariffStrategy : ITariffStrategy
{
    public decimal CalculatePrice(decimal basePrice, int days) => basePrice * days * 1.5m;
}
