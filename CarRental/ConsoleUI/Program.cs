using System;

using System.Linq;
using CarRental.Domain;
using CarRental.Infrastructure;
using CarRental.Application;
using CarRental.Application.Dto;

namespace CarRental.ConsoleUI
{
    // Точка входу
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var carRepo = new FileCarRepository();
            var bookingRepo = new FileBookingRepository();
            var bookingService = new BookingService(carRepo, bookingRepo);

            // Вивід списку авто з номерами
            var cars = carRepo.GetAll().ToList();
            Console.WriteLine("Доступні авто:");
            for (int i = 0; i < cars.Count; i++)
                Console.WriteLine($"{i}. {cars[i].Id} - {cars[i].Model} ({cars[i].Category})");

            Guid carId;
            while (true)
            {
                Console.Write($"Введіть номер авто (0-{cars.Count - 1}) або повний Guid: ");
                string input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int index) && index >= 0 && index < cars.Count)
                {
                    carId = cars[index].Id;
                    break;
                }
                if (Guid.TryParse(input, out Guid guid) && cars.Any(c => c.Id == guid))
                {
                    carId = guid;
                    break;
                }
                Console.WriteLine("Некоректний номер або Guid. Спробуйте ще раз.");
            }

            DateOnly startDate, endDate;
            while (true)
            {
                Console.Write("Дата початку (yyyy-MM-dd): ");
                string input = Console.ReadLine()?.Trim();
                if (DateOnly.TryParse(input, out startDate))
                    break;
                Console.WriteLine("Некоректна дата. Спробуйте ще раз.");
            }
            while (true)
            {
                Console.Write("Дата завершення (yyyy-MM-dd): ");
                string input = Console.ReadLine()?.Trim();
                if (DateOnly.TryParse(input, out endDate))
                    break;
                Console.WriteLine("Некоректна дата. Спробуйте ще раз.");
            }

            // Формуємо DTO для use case
            var request = new BookingRequestDto
            {
                CarId = carId,
                CustomerId = Guid.NewGuid(), // Для демо — випадковий клієнт
                Start = startDate.ToDateTime(TimeOnly.MinValue),
                End = endDate.ToDateTime(TimeOnly.MinValue)
            };

            var result = await bookingService.CreateBookingAsync(request);
            if (result.Success)
                Console.WriteLine($"Бронювання успішне! ID: {result.BookingId}");
            else
                Console.WriteLine($"Помилка: {result.Message}");
        }
    }
}
