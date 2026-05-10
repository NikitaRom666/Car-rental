using System;
using CarRental.Domain;
using Xunit;

namespace CarRental.Tests
{
    public class DomainEntityTests
    {
        // ===== BookingPeriod Tests =====

        [Fact]
        public void BookingPeriod_ValidDates_Created()
        {
            var start = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(3));

            var period = new BookingPeriod(start, end);

            Assert.Equal(start, period.Start);
            Assert.Equal(end, period.End);
            Assert.Equal(2, period.DurationDays);
        }

        [Fact]
        public void BookingPeriod_StartEqualEnd_ThrowsException()
        {
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            var ex = Assert.Throws<ArgumentException>(() => new BookingPeriod(date, date));
            Assert.Contains("дата завершення має бути після початку", ex.Message);
        }

        [Fact]
        public void BookingPeriod_EndBeforeStart_ThrowsException()
        {
            var start = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            var ex = Assert.Throws<ArgumentException>(() => new BookingPeriod(start, end));
            Assert.Contains("дата завершення має бути після початку", ex.Message);
        }

        [Fact]
        public void BookingPeriod_Overlaps_ReturnsTrueForIntersectingPeriods()
        {
            var period1 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
            var period2 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(3)), DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

            Assert.True(period1.Overlaps(period2));
            Assert.True(period2.Overlaps(period1));
        }

        [Fact]
        public void BookingPeriod_Overlaps_ReturnsFalseForNonIntersectingPeriods()
        {
            var period1 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            var period2 = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(5)), DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

            Assert.False(period1.Overlaps(period2));
            Assert.False(period2.Overlaps(period1));
        }

        [Fact]
        public void BookingPeriod_Overlaps_ThrowsOnNullPeriod()
        {
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentNullException>(() => period.Overlaps(null));
            Assert.Equal("other", ex.ParamName);
        }

        // ===== Car Tests =====

        [Fact]
        public void Car_ValidData_Created()
        {
            var id = Guid.NewGuid();
            var model = "Toyota Yaris";
            var category = VehicleCategory.Economy;

            var car = new Car(id, model, category);

            Assert.Equal(id, car.Id);
            Assert.Equal(model, car.Model);
            Assert.Equal(category, car.Category);
            Assert.Equal(AvailabilityStatus.Available, car.AvailabilityStatus);
            Assert.True(car.IsAvailable);
        }

        [Fact]
        public void Car_EmptyId_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Car(Guid.Empty, "Model", VehicleCategory.Economy));
            Assert.Contains("ID авто не може бути порожнім", ex.Message);
        }

        [Fact]
        public void Car_NullModel_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Car(Guid.NewGuid(), null, VehicleCategory.Economy));
            Assert.Contains("Модель авто не може бути порожньою", ex.Message);
        }

        [Fact]
        public void Car_EmptyModel_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Car(Guid.NewGuid(), "   ", VehicleCategory.Economy));
            Assert.Contains("Модель авто не може бути порожньою", ex.Message);
        }

        [Fact]
        public void Car_MarkBooked_ChangesStatus()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);

            car.MarkBooked();

            Assert.Equal(AvailabilityStatus.Booked, car.AvailabilityStatus);
            Assert.False(car.IsAvailable);
        }

        [Fact]
        public void Car_MarkAvailable_ChangesStatus()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Booked);

            car.MarkAvailable();

            Assert.Equal(AvailabilityStatus.Available, car.AvailabilityStatus);
            Assert.True(car.IsAvailable);
        }

        [Fact]
        public void Car_MarkBooked_WhenUnavailable_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Unavailable);

            var ex = Assert.Throws<InvalidOperationException>(() => car.MarkBooked());
            Assert.Contains("Неможливо забронювати недоступне авто", ex.Message);
        }

        [Fact]
        public void Car_MarkAvailable_WhenUnavailable_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy, AvailabilityStatus.Unavailable);

            var ex = Assert.Throws<InvalidOperationException>(() => car.MarkAvailable());
            Assert.Contains("Неможливо відкрити недоступне авто", ex.Message);
        }

        // ===== Booking Tests =====

        [Fact]
        public void Booking_ValidData_Created()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Іван Петренко");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            var price = 150m;

            var booking = new Booking(Guid.NewGuid(), car, customer, period, price);

            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.Equal(car, booking.Car);
            Assert.Equal(customer, booking.Customer);
            Assert.Equal(period, booking.Period);
            Assert.Equal(price, booking.Price);
            Assert.Equal(BookingStatus.Active, booking.Status);
            Assert.True(booking.IsActive);
        }

        [Fact]
        public void Booking_EmptyId_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentException>(() => new Booking(Guid.Empty, car, customer, period, 100m));
            Assert.Contains("ID бронювання не може бути порожнім", ex.Message);
        }

        [Fact]
        public void Booking_NullCar_ThrowsException()
        {
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentNullException>(() => new Booking(Guid.NewGuid(), null, customer, period, 100m));
            Assert.Equal("car", ex.ParamName);
        }

        [Fact]
        public void Booking_NullCustomer_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentNullException>(() => new Booking(Guid.NewGuid(), car, null, period, 100m));
            Assert.Equal("customer", ex.ParamName);
        }

        [Fact]
        public void Booking_NullPeriod_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");

            var ex = Assert.Throws<ArgumentNullException>(() => new Booking(Guid.NewGuid(), car, customer, null, 100m));
            Assert.Equal("period", ex.ParamName);
        }

        [Fact]
        public void Booking_NegativePrice_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Booking(Guid.NewGuid(), car, customer, period, -100m));
            Assert.Contains("Ціна має бути більшою за нуль", ex.Message);
        }

        [Fact]
        public void Booking_ZeroPrice_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Booking(Guid.NewGuid(), car, customer, period, 0m));
            Assert.Contains("Ціна має бути більшою за нуль", ex.Message);
        }

        [Fact]
        public void Booking_Cancel_ChangesStatus()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            var booking = new Booking(Guid.NewGuid(), car, customer, period, 100m);

            booking.Cancel();

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.False(booking.IsActive);
        }

        [Fact]
        public void Booking_CancelAlreadyCancelled_ThrowsException()
        {
            var car = new Car(Guid.NewGuid(), "Test", VehicleCategory.Economy);
            var customer = new Customer(Guid.NewGuid(), "Test");
            var period = new BookingPeriod(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            var booking = new Booking(Guid.NewGuid(), car, customer, period, 100m);

            booking.Cancel();
            var ex = Assert.Throws<InvalidOperationException>(() => booking.Cancel());

            Assert.Contains("Бронювання вже скасоване", ex.Message);
        }

        // ===== Customer Tests =====

        [Fact]
        public void Customer_ValidData_Created()
        {
            var id = Guid.NewGuid();
            var name = "Іван Петренко";

            var customer = new Customer(id, name);

            Assert.Equal(id, customer.Id);
            Assert.Equal(name, customer.Name);
        }

        [Fact]
        public void Customer_EmptyId_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Customer(Guid.Empty, "Test"));
            Assert.Contains("ID клієнта не може бути порожнім", ex.Message);
        }

        [Fact]
        public void Customer_NullName_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Customer(Guid.NewGuid(), null));
            Assert.Contains("Ім'я клієнта не може бути порожнім", ex.Message);
        }

        [Fact]
        public void Customer_EmptyName_ThrowsException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Customer(Guid.NewGuid(), "  "));
            Assert.Contains("Ім'я клієнта не може бути порожнім", ex.Message);
        }
    }
}
