# Iteration 1: Domain Architecture & Vertical Slice

## Implemented
- Domain entities: Car, Booking, Customer, BookingPeriod
- Interfaces: ICarRepository, IBookingRepository
- Application service: BookingService (availability + overlap checks)
- Infrastructure: FileCarRepository, FileBookingRepository (JSON persistence)
- Console UI: One vertical slice (select car → validate → create booking → persist)
- Tests: Basic unit tests for booking creation
- CI: GitHub Actions workflow (build + test)

## Known Risks
- JSON persistence without corruption handling
- No detailed repository contract documentation
- Limited test coverage

## Next Steps
- Add pricing strategies (Lab 35)
- Add LINQ queries for analytics (Lab 35)
- Expand test suite to 20+ unit + 8+ integration (Lab 36)
- Add fault tolerance (Lab 36)
