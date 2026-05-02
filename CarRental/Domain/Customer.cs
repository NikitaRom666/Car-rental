using System;

namespace CarRental.Domain
{
    // Сутність клієнта
    public class Customer
    {
        public Guid Id { get; }
        public string Name { get; }

        public Customer(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
