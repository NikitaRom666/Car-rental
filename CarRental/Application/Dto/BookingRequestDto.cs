using System;

namespace CarRental.Application.Dto
{
    public class BookingRequestDto
    {
        public Guid CarId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
