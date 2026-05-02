using System;

namespace CarRental.Domain
{
    // Сутність авто
    public class Car
    {
        public Guid Id { get; }
        public string Model { get; }
        public VehicleCategory Category { get; }
        public AvailabilityStatus AvailabilityStatus { get; private set; }

        public Car(Guid id, string model, VehicleCategory category, AvailabilityStatus status = AvailabilityStatus.Available)
        {
            Id = id;
            Model = model;
            Category = category;
            AvailabilityStatus = status;
        }

        // Змінити статус доступності
        public void SetAvailability(AvailabilityStatus status)
        {
            AvailabilityStatus = status;
        }
    }

    // Статус доступності авто
    public enum AvailabilityStatus
    {
        Available,
        Booked,
        Unavailable
    }
}
