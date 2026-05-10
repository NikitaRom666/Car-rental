using System;
using CarRental.Domain;
using CarRental.Application.Dto;
using System.Linq;
using CarRental.Application.Strategies;

namespace CarRental.Application
{
    public class BookingService
    {
        private readonly ICarRepository _carRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly BookingPricingStrategyFactory _pricingStrategyFactory;

        public BookingService(ICarRepository carRepo, IBookingRepository bookingRepo)
            : this(carRepo, bookingRepo, new BookingPricingStrategyFactory())
        {
        }

        public BookingService(ICarRepository carRepo, IBookingRepository bookingRepo, BookingPricingStrategyFactory pricingStrategyFactory)
        {
            _carRepo = carRepo;
            _bookingRepo = bookingRepo;
            _pricingStrategyFactory = pricingStrategyFactory;
        }

        public BookingResultDto CreateBooking(BookingRequestDto? request)
        {
            return CreateBookingAsync(request).GetAwaiter().GetResult();
        }

        public async System.Threading.Tasks.Task<BookingResultDto> CreateBookingAsync(BookingRequestDto? request)
        {
            if (request == null)
                return Fail("Запит не може бути порожнім");
            if (request.CarId == Guid.Empty)
                return Fail("ID авто не задано");
            if (request.CustomerId == Guid.Empty)
                return Fail("ID клієнта не задано");
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                return Fail("Ім'я клієнта не задано");

            if (!IsCustomerEligibleForBooking(request.CustomerId))
                return Fail("Досягнута максимальна кількість активних бронювань для цього клієнта");

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

            var overlaps = await _bookingRepo.GetOverlappingAsync(request.CarId, startDate, endDate);
            if (overlaps.Any())
                return Fail("Авто вже заброньоване на цей період");

            var period = new BookingPeriod(startDate, endDate);
            var customer = new Customer(request.CustomerId, request.CustomerName);
            var strategy = _pricingStrategyFactory.GetStrategy(car.Category);
            var price = strategy.CalculatePrice(car, period);
            var booking = new Booking(Guid.NewGuid(), car, customer, period, price);
            _bookingRepo.Add(booking);
            car.MarkBooked();
            _carRepo.Update(car);

            return Success(booking.Id);
        }

        public async System.Threading.Tasks.Task<BookingResultDto> CancelBookingAsync(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
                return Fail("ID бронювання не задано");

            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null)
                return Fail("Бронювання не знайдено");
            if (!booking.IsActive)
                return Fail("Бронювання вже скасоване");
            if (booking.Period.Start <= DateOnly.FromDateTime(DateTime.Today))
                return Fail("Не можна скасувати бронювання, яке вже почалося");

            booking.Cancel();
            booking.Car.MarkAvailable();
            _bookingRepo.Update(booking);
            _carRepo.Update(booking.Car);
            await System.Threading.Tasks.Task.CompletedTask;

            return new BookingResultDto
            {
                Success = true,
                Message = "Бронювання скасовано",
                BookingId = booking.Id
            };
        }

        public int GetActiveBookingCountForCustomer(Guid customerId)
        {
            return _bookingRepo.GetAll()
                .Count(booking => booking.Customer.Id == customerId && booking.IsActive);
        }

        public bool IsCustomerEligibleForBooking(Guid customerId)
        {
            const int maxActiveBookings = 3;
            var activeCount = GetActiveBookingCountForCustomer(customerId);
            return activeCount < maxActiveBookings;
        }

        private BookingResultDto Fail(string message) => new() { Success = false, Message = message };
        private BookingResultDto Success(Guid id) => new() { Success = true, Message = "Бронювання успішне!", BookingId = id };
    }
}
