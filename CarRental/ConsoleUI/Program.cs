using System;
using System.Globalization;
using System.Linq;
using CarRental.Application;
using CarRental.Application.Dto;
using CarRental.Domain;
using CarRental.Infrastructure;

namespace CarRental.ConsoleUI
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var carRepository = new FileCarRepository();
            var bookingRepository = new FileBookingRepository();
            await carRepository.LoadAsync();
            await bookingRepository.LoadAsync();

            var bookingService = new BookingService(carRepository, bookingRepository);
            var queryService = new BookingQueryService(carRepository, bookingRepository);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Car Rental ===");
                Console.WriteLine("1. Показати авто");
                Console.WriteLine("2. Створити бронювання");
                Console.WriteLine("3. Скасувати бронювання");
                Console.WriteLine("4. Показати активні бронювання");
                Console.WriteLine("5. Пошук бронювань");
                Console.WriteLine("6. Аналітика");
                Console.WriteLine("7. Додати авто");
                Console.WriteLine("0. Вихід");
                Console.Write("Вибір: ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ShowCars(queryService, carRepository);
                        break;
                    case "2":
                        await CreateBookingAsync(carRepository, bookingService);
                        break;
                    case "3":
                        await CancelBookingAsync(bookingService);
                        break;
                    case "4":
                        ShowActiveBookings(queryService);
                        break;
                    case "5":
                        SearchBookings(queryService);
                        break;
                    case "6":
                        ShowAnalytics(queryService);
                        break;
                    case "7":
                        AddCar(carRepository);
                        break;
                    case "0":
                        await carRepository.SaveAsync();
                        await bookingRepository.SaveAsync();
                        return;
                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }

        private static void ShowCars(BookingQueryService queryService, ICarRepository carRepository)
        {
            var cars = carRepository.GetAll().ToList();
            Console.WriteLine();
            Console.WriteLine("Доступні авто в системі:");
            foreach (var car in cars)
                Console.WriteLine($"{car.Id} | {car.Model} | {car.Category} | {car.AvailabilityStatus}");
        }

        private static async System.Threading.Tasks.Task CreateBookingAsync(ICarRepository carRepository, BookingService bookingService)
        {
            var cars = carRepository.GetAll().ToList();
            Console.WriteLine();
            Console.WriteLine("Оберіть авто:");
            for (var index = 0; index < cars.Count; index++)
                Console.WriteLine($"{index}. {cars[index].Model} ({cars[index].Category}) - {cars[index].AvailabilityStatus}");

            var car = ReadCar(cars);
            if (car == null)
                return;

            var customerName = ReadRequiredText("Ім'я клієнта: ");
            var start = ReadDate("Дата початку (yyyy-MM-dd): ");
            var end = ReadDate("Дата завершення (yyyy-MM-dd): ");

            var request = new BookingRequestDto
            {
                CarId = car.Id,
                CustomerId = Guid.NewGuid(),
                CustomerName = customerName,
                Start = start.ToDateTime(TimeOnly.MinValue),
                End = end.ToDateTime(TimeOnly.MinValue)
            };

            var result = await bookingService.CreateBookingAsync(request);
            PrintResult(result);
        }

        private static async System.Threading.Tasks.Task CancelBookingAsync(BookingService bookingService)
        {
            var bookingId = ReadGuid("Введіть ID бронювання: ");
            var result = await bookingService.CancelBookingAsync(bookingId);
            PrintResult(result);
        }

        private static void ShowActiveBookings(BookingQueryService queryService)
        {
            var bookings = queryService.GetActiveBookings();
            Console.WriteLine();
            Console.WriteLine("Активні бронювання:");
            if (!bookings.Any())
            {
                Console.WriteLine("Немає активних бронювань.");
                return;
            }

            foreach (var booking in bookings)
                Console.WriteLine($"{booking.Id} | {booking.CustomerName} | {booking.CarModel} | {booking.Start:yyyy-MM-dd} - {booking.End:yyyy-MM-dd} | {booking.Price}");
        }

        private static void SearchBookings(BookingQueryService queryService)
        {
            var text = ReadRequiredText("Введіть частину імені клієнта або моделі авто: ");
            var bookings = queryService.SearchBookings(text);

            Console.WriteLine();
            Console.WriteLine("Результати пошуку:");
            if (!bookings.Any())
            {
                Console.WriteLine("Нічого не знайдено.");
                return;
            }

            foreach (var booking in bookings)
                Console.WriteLine($"{booking.Id} | {booking.CustomerName} | {booking.CarModel} | {booking.Status}");
        }

        private static void ShowAnalytics(BookingQueryService queryService)
        {
            Console.WriteLine();
            Console.WriteLine("1. Доступні авто");
            Console.WriteLine("2. Топ авто за кількістю бронювань");
            Console.WriteLine("3. Дохід по категоріях");
            Console.Write("Вибір: ");

            var choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1":
                    var availableCars = queryService.GetAvailableCars();
                    Console.WriteLine("Доступні авто:");
                    foreach (var car in availableCars)
                        Console.WriteLine($"{car.Model} | {car.Category}");
                    break;
                case "2":
                    var popularCars = queryService.GetMostPopularCars();
                    Console.WriteLine("Топ авто:");
                    foreach (var car in popularCars)
                        Console.WriteLine($"{car.Model} | бронювань: {car.BookingCount}");
                    break;
                case "3":
                    var revenue = queryService.GetRevenueByCategory();
                    Console.WriteLine("Дохід по категоріях:");
                    foreach (var item in revenue)
                        Console.WriteLine($"{item.Key}: {item.Value}");
                    break;
                default:
                    Console.WriteLine("Невірний вибір.");
                    break;
            }
        }

        private static void AddCar(ICarRepository carRepository)
        {
            var model = ReadRequiredText("Модель авто: ");
            var category = ReadCategory("Категорія (Economy / Standard / SUV): ");
            var car = new Car(Guid.NewGuid(), model, category, AvailabilityStatus.Available);
            carRepository.Add(car);
            Console.WriteLine($"Авто додано. ID: {car.Id}");
        }

        private static Car ReadCar(System.Collections.Generic.List<Car> cars)
        {
            while (true)
            {
                Console.Write("Номер авто або Guid: ");
                var input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out var index) && index >= 0 && index < cars.Count)
                    return cars[index];

                if (Guid.TryParse(input, out var carId))
                {
                    var car = cars.FirstOrDefault(item => item.Id == carId);
                    if (car != null)
                        return car;
                }

                Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
            }
        }

        private static string ReadRequiredText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;

                Console.WriteLine("Поле не може бути порожнім.");
            }
        }

        private static DateOnly ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = Console.ReadLine()?.Trim();
                if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    return date;

                Console.WriteLine("Некоректна дата.");
            }
        }

        private static Guid ReadGuid(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = Console.ReadLine()?.Trim();
                if (Guid.TryParse(value, out var id))
                    return id;

                Console.WriteLine("Некоректний Guid.");
            }
        }

        private static VehicleCategory ReadCategory(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = Console.ReadLine()?.Trim();
                if (Enum.TryParse<VehicleCategory>(value, true, out var category))
                    return category;

                Console.WriteLine("Некоректна категорія.");
            }
        }

        private static void PrintResult(BookingResultDto result)
        {
            if (result.Success)
                Console.WriteLine($"Успішно: {result.Message} ID: {result.BookingId}");
            else
                Console.WriteLine($"Помилка: {result.Message}");
        }
    }
}
