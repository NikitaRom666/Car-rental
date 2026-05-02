using System;

namespace CarRental.Domain
{
    // Сутність бронювання
    public class Booking
    {
        public Guid Id { get; }
        public Car Car { get; }
        public Customer Customer { get; }
        public BookingPeriod Period { get; }
        public decimal Price { get; }
        public BookingStatus Status { get; private set; }

        public bool IsActive => Status == BookingStatus.Active;

        public Booking(Guid id, Car car, Customer customer, BookingPeriod period, decimal price, BookingStatus status = BookingStatus.Active)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID бронювання не може бути порожнім", nameof(id));
            if (car == null)
                throw new ArgumentNullException(nameof(car));
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            if (period == null)
                throw new ArgumentNullException(nameof(period));
            if (price <= 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Ціна має бути більшою за нуль");

            Id = id;
            Car = car;
            Customer = customer;
            Period = period;
            Price = price;
            Status = status;
        }

        public void Cancel()
        {
            if (!IsActive)
                throw new InvalidOperationException("Бронювання вже скасоване");

            Status = BookingStatus.Cancelled;
        }
    }
}
