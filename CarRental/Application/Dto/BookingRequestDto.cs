using System;

namespace CarRental.Application.Dto
{
    // DTO для запиту на бронювання
    public class BookingRequestDto
    {
        public Guid CarId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
