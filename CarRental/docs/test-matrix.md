# Test Matrix

| Use case | Unit tests | Integration tests |
| --- | --- | --- |
| Створення бронювання | `CreateBooking_Success`, `CreateBooking_UnavailableCar_ReturnsError`, `CreateBooking_InvalidDateRange_ReturnsError` | `BookingService_WithRealRepositories_PersistsBookingAndUpdatesCar` |
| Скасування бронювання | `CancelBooking_Success_ReleasesCar` | `BookingRepository_AddAndReload_PreservesBooking` |
| Перевірка інваріантів | `Car_EmptyModel_Throws`, `Customer_EmptyName_Throws`, `BookingPeriod_InvalidRange_Throws` | `CarRepository_SaveAndReload_PreservesCars` |
| Strategy ціноутворення | `PricingStrategyFactory_ReturnsCorrectStrategies` | `BookingService_WithRealRepositories_PersistsBookingAndUpdatesCar` |
| Пошук і аналітика | `QueryService_ReturnsActiveBookingsSortedByStartDate`, `QueryService_PopularityReport_UsesBookingCounts`, `QueryService_RevenueByCategory_GroupsTotals` | `QueryService` через відновлений стан |
