using System;
using CarRental.Domain;
using CarRental.Application.Dto;
using System.Linq;

namespace CarRental.Application
{
    // Сервіс для бронювання авто
    public class BookingService
    {
        private readonly ICarRepository _carRepo;
        private readonly IBookingRepository _bookingRepo;

        public BookingService(ICarRepository carRepo, IBookingRepository bookingRepo)
        {
            _carRepo = carRepo;
            _bookingRepo = bookingRepo;
        }

        public BookingResultDto CreateBooking(BookingRequestDto request)
        {
            return CreateBookingAsync(request).GetAwaiter().GetResult();
        }

        public async System.Threading.Tasks.Task<BookingResultDto> CreateBookingAsync(BookingRequestDto request)
        {
            // Перевірка на null
            if (request == null)
                return Fail("Запит не може бути порожнім");
            if (request.CarId == Guid.Empty)
                return Fail("ID авто не задано");
            if (request.CustomerId == Guid.Empty)
                return Fail("ID клієнта не задано");

            // Перевірка дат (DateOnly)
            if (request.Start == default || request.End == default)
                return Fail("Дати не можуть бути порожніми");
            var startDate = DateOnly.FromDateTime(request.Start);
            var endDate = DateOnly.FromDateTime(request.End);
            if (startDate < new DateOnly(2026, 1, 1))
                return Fail("Дата не може бути раніше 2026 року");
            if (startDate >= endDate)
                return Fail("Дата завершення має бути після початку");
            if ((endDate.DayNumber - startDate.DayNumber) < 1)
                return Fail("Мінімальний період бронювання — 1 день");

            var car = _carRepo.GetById(request.CarId);
            if (car == null)
                return Fail("Авто не знайдено");
            if (car.AvailabilityStatus != AvailabilityStatus.Available)
                return Fail("Авто тимчасово недоступне");

            // Перевірка перетинів
            var overlaps = await _bookingRepo.GetOverlappingAsync(request.CarId, startDate, endDate);
            if (overlaps.Any())
                return Fail("Авто вже заброньоване на цей період");

            // Створюємо бронювання
            var period = new BookingPeriod(startDate, endDate);
            var customer = new Customer(request.CustomerId, "Test");
            var days = endDate.DayNumber - startDate.DayNumber;
            var price = days * 100m;
            var booking = new Booking(Guid.NewGuid(), car, customer, period, price);
            await System.Threading.Tasks.Task.Run(() => _bookingRepo.Add(booking));
            car.SetAvailability(AvailabilityStatus.Booked);
            await System.Threading.Tasks.Task.Run(() => _carRepo.Update(car));

            return Success(booking.Id);
        }

        private BookingResultDto Fail(string message) => new() { Success = false, Message = message };
        private BookingResultDto Success(Guid id) => new() { Success = true, Message = "Бронювання успішне!", BookingId = id };
    }
}
