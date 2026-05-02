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

        public Booking(Guid id, Car car, Customer customer, BookingPeriod period, decimal price)
        {
            Id = id;
            Car = car;
            Customer = customer;
            Period = period;
            Price = price;
        }
    }
}
