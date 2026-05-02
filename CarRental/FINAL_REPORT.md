# Фінальний звіт

## Про що проєкт
CarRental — це навчальний проєкт з оренди авто. Я зробив його як просту багатошарову систему, щоб показати доменну модель, логіку бронювання, роботу з файлами і тести.

## Що зроблено
- Зроблено домен: [Car](Domain/Car.cs), [Customer](Domain/Customer.cs), [Booking](Domain/Booking.cs), [BookingPeriod](Domain/BookingPeriod.cs), [VehicleCategory](Domain/VehicleCategory.cs), [AvailabilityStatus](Domain/Car.cs)
- Зроблено use case бронювання в [BookingService](Application/BookingService.cs)
- Зроблено DTO для запиту і результату бронювання: [BookingRequestDto](Application/Dto/BookingRequestDto.cs), [BookingResultDto](Application/Dto/BookingResultDto.cs)
- Зроблено файлові репозиторії: [FileCarRepository](Infrastructure/FileCarRepository.cs), [FileBookingRepository](Infrastructure/FileBookingRepository.cs)
- Зроблено валідацію бронювання в [BookingValidator](Domain/Validators/BookingValidator.cs)
- Зроблено консольний інтерфейс у [Program](ConsoleUI/Program.cs)
- Зроблено unit та integration тести у [Tests](Tests)

## Діаграми

### Діаграма класів
![Діаграма класів](docs/Note%20Management%20Patterns-2026-05-02-074707.png)

### Діаграма послідовності
![Діаграма послідовності](docs/Note%20Management%20Patterns-2026-05-02-074802.png)

### Діаграма архітектури
![Діаграма архітектури](docs/Note%20Management%20Patterns-2026-05-02-074823.png)

## Коротко по результату
- Збірка проходить
- Тести проходять
- Бронювання створюється через консоль
- Дані зберігаються у JSON-файли

## Висновок
Проєкт вийшов як нормальний навчальний приклад: є шари, є логіка, є перевірки і є документація. Все зроблено без зайвого ускладнення.
