namespace CarRental.Application.Services;
using CarRental.Application.DTOs;
using CarRental.Application.Interfaces.Repositories;
using CarRental.Application.Interfaces.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.ValueObjects;

/// <summary>
/// Main rental service handling core business logic.
/// Orchestrates use cases and applies business rules.
/// This implements the Service Layer pattern.
/// </summary>
public class RentalService
{
    private readonly ICarRepository _carRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IRentalEventObserver _eventObserver;
    private readonly Dictionary<int, IPricingStrategy> _pricingStrategies;

    public RentalService(
        ICarRepository carRepository,
        ICustomerRepository customerRepository,
        IRentalRepository rentalRepository,
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        ILocationRepository locationRepository,
        IRentalEventObserver eventObserver)
    {
        _carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _rentalRepository = rentalRepository ?? throw new ArgumentNullException(nameof(rentalRepository));
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));

        _pricingStrategies = new Dictionary<int, IPricingStrategy>
        {
            { (int)Domain.Enums.CarCategory.Economy, new EconomyPricingStrategy() },
            { (int)Domain.Enums.CarCategory.Business, new BusinessPricingStrategy() },
            { (int)Domain.Enums.CarCategory.Premium, new PremiumPricingStrategy() }
        };
    }

    /// <summary>
    /// Use Case 1: Check car availability for a given period.
    /// </summary>
    public async Task<bool> CheckCarAvailabilityAsync(Guid carId, DateRange period)
    {
        var car = await _carRepository.GetByIdAsync(carId)
            ?? throw new NotFoundException(nameof(Car), carId);

        if (!car.IsAvailable)
            return false;

        bool hasConflictingRentals = await _rentalRepository.HasConflictingRentalsAsync(carId, period);
        bool hasConflictingReservations = await _reservationRepository.HasConflictingReservationsAsync(carId, period);

        return !hasConflictingRentals && !hasConflictingReservations;
    }

    /// <summary>
    /// Use Case 2: Create a reservation for future dates.
    /// </summary>
    public async Task<OperationResult<ReservationDto>> CreateReservationAsync(CreateReservationRequest request)
    {
        try
        {
            // Validate entities exist
            var car = await _carRepository.GetByIdAsync(request.CarId)
                ?? throw new NotFoundException(nameof(Car), request.CarId);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
                ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

            // Create date range (validates dates)
            var rentalPeriod = new DateRange(request.StartDate, request.EndDate);

            // Check for conflicts
            bool isAvailable = await CheckCarAvailabilityAsync(request.CarId, rentalPeriod);
            if (!isAvailable)
                throw new BusinessRuleViolationException(
                    $"Car is not available for the requested period ({rentalPeriod})");

            // Create and persist reservation
            var reservation = new Reservation(request.CarId, request.CustomerId, rentalPeriod);
            await _reservationRepository.AddAsync(reservation);

            var dto = MapToDto(reservation);
            return OperationResult<ReservationDto>.CreateSuccess("Reservation created successfully.", dto);
        }
        catch (Exception ex)
        {
            return OperationResult<ReservationDto>.CreateFailure(ex.Message);
        }
    }

    /// <summary>
    /// Use Case 3: Start a rental (create and activate).
    /// </summary>
    public async Task<OperationResult<RentalDto>> StartRentalAsync(CreateRentalRequest request)
    {
        try
        {
            // Validate entities
            var car = await _carRepository.GetByIdAsync(request.CarId)
                ?? throw new NotFoundException(nameof(Car), request.CarId);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId)
                ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

            // Validate car is available (throws if not)
            car.ValidateAvailability();

            // Create date range and validate no conflicts
            var rentalPeriod = new DateRange(request.StartDate, request.EndDate);
            bool isAvailable = await CheckCarAvailabilityAsync(request.CarId, rentalPeriod);
            
            if (!isAvailable)
                throw new BusinessRuleViolationException(
                    "Cannot start rental: car has conflicting bookings.");

            // Calculate price using strategy pattern
            var pricingStrategy = _pricingStrategies[(int)car.Category];
            Money totalPrice = pricingStrategy.CalculatePrice(car.PricePerDay, rentalPeriod.DaysCount);

            // Create rental
            var rental = new Rental(request.CarId, request.CustomerId, rentalPeriod, totalPrice);
            rental.Activate();

            // Mark car as unavailable
            car.MakeUnavailable();

            // Persist changes
            await _rentalRepository.AddAsync(rental);
            await _carRepository.UpdateAsync(car);

            // Create payment record
            var payment = new Payment(rental.Id, totalPrice);
            await _paymentRepository.AddAsync(payment);

            // Notify observers
            await _eventObserver.OnRentalCreatedAsync(rental.Id, customer.Id, car.Id);
            await _eventObserver.OnRentalStartedAsync(rental.Id);

            var dto = MapToDto(rental);
            return OperationResult<RentalDto>.CreateSuccess(
                $"Rental started successfully. Total price: {totalPrice}", dto);
        }
        catch (Exception ex)
        {
            return OperationResult<RentalDto>.CreateFailure(ex.Message);
        }
    }

    /// <summary>
    /// Use Case 4: Complete an active rental.
    /// </summary>
    public async Task<OperationResult<RentalDto>> FinishRentalAsync(Guid rentalId)
    {
        try
        {
            var rental = await _rentalRepository.GetByIdAsync(rentalId)
                ?? throw new NotFoundException(nameof(Rental), rentalId);

            // Business rule validation
            rental.Complete();

            var car = await _carRepository.GetByIdAsync(rental.CarId)
                ?? throw new NotFoundException(nameof(Car), rental.CarId);

            // Mark car as available again
            car.MakeAvailable();

            // Mark payment as paid
            var payments = await _paymentRepository.GetByRentalIdAsync(rentalId);
            if (payments.Any())
            {
                var payment = payments.First();
                payment.MarkAsPaid($"TXN-{rentalId:N}");
                await _paymentRepository.UpdateAsync(payment);
            }

            // Persist changes
            await _rentalRepository.UpdateAsync(rental);
            await _carRepository.UpdateAsync(car);

            // Notify observers
            await _eventObserver.OnRentalCompletedAsync(rental.Id);

            var dto = MapToDto(rental);
            return OperationResult<RentalDto>.CreateSuccess("Rental completed successfully.", dto);
        }
        catch (Exception ex)
        {
            return OperationResult<RentalDto>.CreateFailure(ex.Message);
        }
    }

    /// <summary>
    /// Helper: Get all available cars.
    /// </summary>
    public async Task<IEnumerable<CarDto>> GetAvailableCarsAsync()
    {
        var cars = await _carRepository.GetAvailableCarsAsync();
        return cars.Select(MapToDto);
    }

    /// <summary>
    /// Helper: Get all cars.
    /// </summary>
    public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
    {
        var cars = await _carRepository.GetAllAsync();
        return cars.Select(MapToDto);
    }

    /// <summary>
    /// Helper: Get rental details.
    /// </summary>
    public async Task<RentalDto?> GetRentalAsync(Guid rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId);
        return rental != null ? MapToDto(rental) : null;
    }

    // Mapping helpers
    private CarDto MapToDto(Car car) => new()
    {
        Id = car.Id,
        Model = car.Model,
        Category = car.Category,
        PricePerDay = car.PricePerDay.Amount,
        IsAvailable = car.IsAvailable,
        LicensePlate = car.LicensePlate,
        ManufacturingYear = car.ManufacturingYear
    };

    private RentalDto MapToDto(Rental rental) => new()
    {
        Id = rental.Id,
        CarId = rental.CarId,
        CustomerId = rental.CustomerId,
        StartDate = rental.RentalPeriod.StartDate,
        EndDate = rental.RentalPeriod.EndDate,
        DaysCount = rental.RentalPeriod.DaysCount,
        TotalPrice = rental.TotalPrice.Amount,
        Status = rental.Status,
        CreatedAt = rental.CreatedAt,
        CompletedAt = rental.CompletedAt
    };

    private ReservationDto MapToDto(Reservation reservation) => new()
    {
        Id = reservation.Id,
        CarId = reservation.CarId,
        CustomerId = reservation.CustomerId,
        StartDate = reservation.ReservationPeriod.StartDate,
        EndDate = reservation.ReservationPeriod.EndDate,
        DaysCount = reservation.ReservationPeriod.DaysCount,
        IsActive = reservation.IsActive,
        CreatedAt = reservation.CreatedAt
    };
}
