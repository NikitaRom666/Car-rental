# Довідник розробника

## Запуск і тестування

```bash
dotnet run --project ConsoleUI/ConsoleUI.csproj
dotnet test
```

## Архітектура

Проєкт реалізований за принципом багатошарової архітектури. Domain шар містить основні сутності (Car, Booking, Customer, BookingPeriod), enum-и статусів, валідатори та контракти репозиторіїв (ICarRepository, IBookingRepository). Вся бізнес-логіка інкапсульована на рівні Domain — конструктори сутностей перевіряють інваріанти, методи виконують переходи стану.

Application шар реалізує use cases через BookingService та BookingQueryService. Ціноутворення для різних категорій авто (Economy, Standard, SUV) здійснюється паттерном Strategy: кожна категорія має свою реалізацію IBookingPricingStrategy, фабрика вибирає стратегію за категорією. LINQ-запити в BookingQueryService агрегують дані: пошук активних бронювань, популярні авто, дохід по категоріях.

Infrastructure містить FileCarRepository та FileBookingRepository з асинхронним I/O. LoadAsync/SaveAsync читають і записують JSON-файли, обробляють помилки через PersistenceException (FileNotFound, UnauthorizedAccess, JsonException, IOException).

ConsoleUI — консольне меню для взаємодії з користувачем через use cases Application шару.

## Ключові компоненти

**Date Overlap Detection**: GetOverlappingAsync перевіряє перетини дат бронювань на одне авто, блокує конфліктні бронювання.

**Pricing Strategies**: Economy = 90/день, Standard = 120/день, SUV = 170/день. Розширення нових тарифів — додати клас, спадкуючий IBookingPricingStrategy, і зареєструвати в BookingPricingStrategyFactory.

**Fault Tolerance**: Помилки файлового I/O (відсутність файлу, пошкоджений JSON, запобіг доступу) не валять програму, а кидають PersistenceException з детальним повідомленням.

**Testing**: 65+ unit-тестів (Moq для репозиторіїв) + 8+ інтеграційних тестів (реальний файловий I/O). xUnit з AAA pattern (Arrange, Act, Assert). Coverage 70%+ для критичного коду.

## Правила розробки

- Вся бізнес-логіка (перевірка конфліктів, розрахунок ціни, валідація періодів) на Domain/Application, не в консолі.
- Domain сутності не мають залежностей на Application чи Infrastructure.
- Application залежить на Domain, Infrastructure залежить на Domain (не навпаки).
- Тести для Business Rules мають бути unit-тести через Moq, тести для I/O — integration з реальним файлом.
