using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarRental.Domain
{
    // Контракт для роботи з авто
    public interface ICarRepository
    {
        Car? GetById(Guid id);
        IEnumerable<Car> GetAll();
        void Add(Car car);
        void Update(Car car);
        Task LoadAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
