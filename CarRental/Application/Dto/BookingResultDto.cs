using System;

namespace CarRental.Application.Dto
{
    public class BookingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? BookingId { get; set; } // ID бронювання (якщо успіх)
    }
}
