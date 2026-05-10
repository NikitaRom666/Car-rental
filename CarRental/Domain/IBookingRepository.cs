using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarRental.Domain
{
    public interface IBookingRepository
    {
        Booking? GetById(Guid id);
        IEnumerable<Booking> GetAll();
        void Add(Booking booking);
        void Update(Booking booking);
        IEnumerable<Booking> GetByCar(Guid carId);

        Task<IEnumerable<Booking>> GetOverlappingAsync(Guid carId, DateOnly start, DateOnly end);
        Task LoadAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
