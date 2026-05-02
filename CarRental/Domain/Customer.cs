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
            if (id == Guid.Empty)
                throw new ArgumentException("ID клієнта не може бути порожнім", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я клієнта не може бути порожнім", nameof(name));

            Id = id;
            Name = name.Trim();
        }
    }
}
