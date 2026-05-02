using System;
using System.Collections.Generic;

namespace CarRental.Domain
{
    // Контракт для роботи з авто
    public interface ICarRepository
    {
        Car GetById(Guid id);
        IEnumerable<Car> GetAll();
        void Update(Car car);
    }
}
