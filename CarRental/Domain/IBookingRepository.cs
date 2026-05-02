using System;
using System.Collections.Generic;

namespace CarRental.Domain
{
    // Контракт для роботи з бронюваннями
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetAll();
        void Add(Booking booking);
        IEnumerable<Booking> GetByCar(Guid carId);

        // Асинхронний пошук перетинів бронювань
        System.Threading.Tasks.Task<IEnumerable<Booking>> GetOverlappingAsync(Guid carId, DateOnly start, DateOnly end);
    }
}
