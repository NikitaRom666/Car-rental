# Iteration 2: LINQ, Pricing Strategies & Fault Handling

## Implemented
- 3+ use cases: booking creation, cancellation, analytics queries
- Strategy pattern: Economy/Standard/SUV pricing strategies
- LINQ queries for analytics aggregation
- BookingQueryService: Analytics layer
- Async I/O: LoadAsync/SaveAsync in repositories
- Test coverage: 20+ unit tests, 8+ integration tests

## Technical Decisions
- Strategy pattern for pricing
- Repository pattern with async I/O
- Domain encapsulates all business rules

## Risks
- Corrupted JSON handling
- Limited edge-case coverage

## Які класи і контракти змінилися
- `BookingService`
- `BookingQueryService`
- `Car`
- `Customer`
- `Booking`
- `BookingPeriod`
- `ICarRepository`
- `IBookingRepository`
- `FileCarRepository`
- `FileBookingRepository`
- `BookingPricingStrategyFactory`
- `BookingPricingStrategy`-класи

## Явні зміни, які треба внести до репозиторію (щоб Lab35 було прозорим)
- Додати `BookingQueryService` з методами: `GetTopCars(int n)`, `GetRevenue(DateOnly start, DateOnly end)`, `GetBookingsCount(DateOnly start, DateOnly end)`.
- Переконатися, що `FileBookingRepository` та `FileCarRepository` реалізують асинхронні `LoadAsync`/`SaveAsync` і коректно поводяться при відсутності/пошкодженні файлу.
- Додати DTO для аналітики та приклади виклику з `ConsoleUI`.
- Додати тест-кейси, що перевіряють, що аналітика дає однакові результати до/після перезапуску (save/load).

## Які сценарії найризикованіші
- пошкоджений або порожній JSON
- повторне скасування вже скасованого бронювання
- конфліктні дані після ручного редагування файлів
- пошук, якщо у системі мало даних

## Що треба буде перевірити на Lab 36
- чи не ламається старт після поганого JSON
- чи зберігається стан після створення і скасування бронювання
- чи працюють запити після перезапуску
- чи не з’явилися регресії в доменних інваріантах
