using System;

namespace CarRental.Domain
{
    public class Car
    {
        public Guid Id { get; }
        public string Model { get; }
        public VehicleCategory Category { get; }
        public AvailabilityStatus AvailabilityStatus { get; private set; }

        public bool IsAvailable => AvailabilityStatus == AvailabilityStatus.Available;

        public Car(Guid id, string model, VehicleCategory category, AvailabilityStatus status = AvailabilityStatus.Available)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID авто не може бути порожнім", nameof(id));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Модель авто не може бути порожньою", nameof(model));

            Id = id;
            Model = model.Trim();
            Category = category;
            AvailabilityStatus = status;
        }

        public void MarkBooked()
        {
            if (AvailabilityStatus == AvailabilityStatus.Unavailable)
                throw new InvalidOperationException("Неможливо забронювати недоступне авто");

            AvailabilityStatus = AvailabilityStatus.Booked;
        }

        public void MarkAvailable()
        {
            if (AvailabilityStatus == AvailabilityStatus.Unavailable)
                throw new InvalidOperationException("Неможливо відкрити недоступне авто");

            AvailabilityStatus = AvailabilityStatus.Available;
        }

        // Для зворотної сумісності
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
