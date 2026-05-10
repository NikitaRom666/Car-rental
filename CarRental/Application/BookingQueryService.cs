using System;
using System.Collections.Generic;
using System.Linq;
using CarRental.Application.Dto;
using CarRental.Domain;

namespace CarRental.Application
{
    public class BookingQueryService
    {
        private readonly ICarRepository _carRepository;
        private readonly IBookingRepository _bookingRepository;

        public BookingQueryService(ICarRepository carRepository, IBookingRepository bookingRepository)
        {
            _carRepository = carRepository;
            _bookingRepository = bookingRepository;
        }

        public IReadOnlyList<BookingSummaryDto> GetActiveBookings()
        {
            return _bookingRepository.GetAll()
                .Where(booking => booking.Status == BookingStatus.Active)
                .OrderBy(booking => booking.Period.Start)
                .Select(ToSummaryDto)
                .ToList();
        }

        public IReadOnlyList<BookingSummaryDto> SearchBookings(string text)
        {
            var normalized = (text ?? string.Empty).Trim();

            return _bookingRepository.GetAll()
                .Where(booking => booking.Customer.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                    || booking.Car.Model.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(booking => booking.Period.Start)
                .Select(ToSummaryDto)
                .ToList();
        }

        public IReadOnlyList<CarSummaryDto> GetAvailableCars()
        {
            return _carRepository.GetAll()
                .Where(car => car.AvailabilityStatus == AvailabilityStatus.Available)
                .OrderBy(car => car.Category)
                .ThenBy(car => car.Model)
                .Select(car => new CarSummaryDto
                {
                    Id = car.Id,
                    Model = car.Model,
                    Category = car.Category,
                    AvailabilityStatus = car.AvailabilityStatus
                })
                .ToList();
        }

        public IReadOnlyList<CarSummaryDto> GetMostPopularCars()
        {
            var bookingCounts = _bookingRepository.GetAll()
                .GroupBy(booking => booking.Car.Id)
                .ToDictionary(group => group.Key, group => group.Count());

            return _carRepository.GetAll()
                .Select(car => new CarSummaryDto
                {
                    Id = car.Id,
                    Model = car.Model,
                    Category = car.Category,
                    AvailabilityStatus = car.AvailabilityStatus,
                    BookingCount = bookingCounts.TryGetValue(car.Id, out var count) ? count : 0
                })
                .OrderByDescending(car => car.BookingCount)
                .ThenBy(car => car.Model)
                .ToList();
        }

        public IReadOnlyDictionary<VehicleCategory, decimal> GetRevenueByCategory()
        {
            return _bookingRepository.GetAll()
                .Where(booking => booking.Status == BookingStatus.Active)
                .GroupBy(booking => booking.Car.Category)
                .ToDictionary(group => group.Key, group => group.Sum(booking => booking.Price));
        }

        public decimal GetTotalRevenueByDateRange(DateOnly start, DateOnly end)
        {
            return _bookingRepository.GetAll()
                .Where(booking => booking.Status == BookingStatus.Active 
                    && booking.Period.Start >= start 
                    && booking.Period.End <= end)
                .Sum(booking => booking.Price);
        }

        public IReadOnlyList<CarSummaryDto> GetCarsBookedByCategory(VehicleCategory category)
        {
            return _carRepository.GetAll()
                .Where(car => car.Category == category && car.AvailabilityStatus == AvailabilityStatus.Unavailable)
                .OrderBy(car => car.Model)
                .Select(car => new CarSummaryDto
                {
                    Id = car.Id,
                    Model = car.Model,
                    Category = car.Category,
                    AvailabilityStatus = car.AvailabilityStatus,
                    BookingCount = _bookingRepository.GetByCar(car.Id).Count()
                })
                .ToList();
        }

        private static BookingSummaryDto ToSummaryDto(Booking booking)
        {
            return new BookingSummaryDto
            {
                Id = booking.Id,
                CarModel = booking.Car.Model,
                Category = booking.Car.Category,
                CustomerName = booking.Customer.Name,
                Start = booking.Period.Start,
                End = booking.Period.End,
                Price = booking.Price,
                Status = booking.Status
            };
        }
    }
}