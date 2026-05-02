using System;
using CarRental.Domain;

namespace CarRental.Application.Dto
{
    public class CarSummaryDto
    {
        public Guid Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public VehicleCategory Category { get; set; }
        public AvailabilityStatus AvailabilityStatus { get; set; }
        public int BookingCount { get; set; }
    }
}