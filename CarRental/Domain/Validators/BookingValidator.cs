using System;
using System.Collections.Generic;
using CarRental.Domain;

namespace CarRental.Domain.Validators
{
    public class ValidationResult
    {
        public bool Success { get; }
        public string Message { get; }
        private ValidationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
        public static ValidationResult Ok() => new ValidationResult(true, "");
        public static ValidationResult Error(string message) => new ValidationResult(false, message);
    }

    public static class BookingValidator
    {
        public static ValidationResult Validate(Booking booking, IEnumerable<Booking> existing)
        {
            if (booking.Car.AvailabilityStatus != AvailabilityStatus.Available)
                return ValidationResult.Error("Авто недоступне для бронювання");
            if (booking.Period.Start >= booking.Period.End)
                return ValidationResult.Error("Некоректний період бронювання");
            if ((booking.Period.End.DayNumber - booking.Period.Start.DayNumber) < 1)
                return ValidationResult.Error("Мінімальний період бронювання — 1 день");
            foreach (var b in existing)
            {
                if (b.Period.Overlaps(booking.Period))
                    return ValidationResult.Error("Авто вже заброньоване на цей період");
            }
            return ValidationResult.Ok();
        }
    }
}
