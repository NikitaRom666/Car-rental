using System;
using CarRental.Domain;

namespace CarRental.Application.Dto
{
    public class BookingSummaryDto
    {
        public Guid Id { get; set; }
        public string CarModel { get; set; } = string.Empty;
        public VehicleCategory Category { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateOnly Start { get; set; }
        public DateOnly End { get; set; }
        public decimal Price { get; set; }
        public BookingStatus Status { get; set; }
    }
}