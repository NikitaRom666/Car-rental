# План ітерації 2

## Що йде в реалізацію
- бронювання авто з ціноутворенням
- скасування бронювання
- пошук і аналітика по бронюваннях та авто
- збереження і відновлення стану з JSON

## Що лишається без змін
- доменні сутності `Car`, `Customer`, `BookingPeriod`
- базова ідея `BookingService`
- структура solution з `Domain`, `Application`, `Infrastructure`, `ConsoleUI`, `Tests`

## Точки розширення
- `BookingPricingStrategyFactory`
- `BookingQueryService`
- `ICarRepository`
- `IBookingRepository`

## Видимі ризики
- можна дублювати правила в console і сервісах, якщо не стежити за межами
- JSON треба акуратно валідовувати, щоб не ламати відновлення
- нові запити можуть стати важкими для тестування, якщо все залишити в Program
