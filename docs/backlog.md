# Product Backlog & User Stories

## Epic 1: Rental Management

### User Story 1.1: View Available Cars
**As a** customer  
**I want to** see all available cars for rent  
**So that** I can choose a vehicle that suits my needs

**Acceptance Criteria:**
- Display shows car model, category, and daily price
- Only available cars are displayed
- Cars are grouped by category (Economy, Business, Premium)
- Display updates when availability changes

**Acceptance Tests:**
- Given cars in system, when user requests available cars, then all available cars displayed
- Given all cars rented, when user requests available cars, then empty list returned
- Given mix of available/unavailable cars, when user requests available cars, then only available shown

**Complexity**: 3 points

---

### User Story 1.2: Start a Rental
**As a** customer  
**I want to** start a rental for a car from a specific date to end date  
**So that** I can use the vehicle for my needs

**Acceptance Criteria:**
- Must select available car
- Must select valid customer
- Must provide rental dates (start < end, minimum 1 day)
- System prevents overlapping rentals
- Car marked unavailable after rental starts
- Rental transitions to Active state
- Payment record created

**Acceptance Tests:**
- Given available car and valid dates, when user starts rental, then rental created and activated
- Given unavailable car, when user tries to start rental, then error returned
- Given overlapping dates, when user tries to start rental, then conflict error
- Given completed start rental, when user checks car, then car marked unavailable

**Complexity**: 5 points

---

### User Story 1.3: Complete a Rental
**As a** customer  
**I want to** complete an active rental  
**So that** I can return the vehicle and finalize payment

**Acceptance Criteria:**
- Only active rentals can be completed
- Car marked available after completion
- Payment marked as completed
- Rental transitions to Completed state
- Completion timestamp recorded

**Acceptance Tests:**
- Given active rental, when user completes rental, then status changes to Completed
- Given inactive rental, when user tries to complete, then error returned
- Given completed rental, when user checks car, then car marked available
- Verify completion timestamp recorded

**Complexity**: 3 points

---

### User Story 1.4: Business Rule - No Double Booking
**As a** system  
**I want to** prevent overlapping rental periods for same car  
**So that** customers don't book same car for same dates

**Acceptance Criteria:**
- Cannot create rental if dates overlap with existing rental
- Cannot create reservation if dates overlap with existing reservation
- Check considers all active/created status rentals
- Clear error message provided

**Acceptance Tests:**
- Given rental for 1/1-1/5, when creating rental 1/3-1/8, then rejected
- Given rental for 1/1-1/5, when creating rental 1/5-1/10, then accepted (no overlap on boundary)
- Given cancelled rental, when creating new rental same dates, then accepted (cancelled not counted)

**Complexity**: 5 points

---

## Epic 2: Reservations & Planning

### User Story 2.1: Create Reservation
**As a** customer  
**I want to** reserve a car for future dates  
**So that** I can ensure car availability when I need it

**Acceptance Criteria:**
- Calculate dates must be in future
- Dates must be valid (start < end, min 1 day)
- System checks for conflicts with other reservations
- System checks for conflicts with rentals
- Reservation created with Active status
- Reservation contains customer, car, and date range

**Acceptance Tests:**
- Given available car and future dates, when user reserves, then reservation created
- Given past dates, when user tries to reserve, then error returned
- Given same dates as existing rental, when user tries to reserve, then error
- Given reserved car, when another customer tries to reserve same dates, then error

**Complexity**: 5 points

---

### User Story 2.2: Cancel Reservation
**As a** customer  
**I want to** cancel a reservation I previously made  
**So that** I can change my plans

**Acceptance Criteria:**
- Only active reservations can be cancelled
- Car availability unchanged by cancellation
- Reservation marked as Cancelled
- Cancellation timestamp recorded

**Acceptance Tests:**
- Given active reservation, when user cancels, then status changes to Cancelled
- Given cancelled reservation, when user tries to cancel again, then error
- Given cancelled reservation, when checking car availability, then car still available (if no rental)

**Complexity**: 2 points

---

## Epic 3: Pricing & Billing

### User Story 3.1: Calculate Rental Price (Economy)
**As a** system  
**I want to** calculate rental price for economy cars  
**So that** customers are charged correctly

**Acceptance Criteria:**
- Economy pricing: flat rate per day (no discount)
- Price = DailyRate × NumberOfDays
- Price rounded to 2 decimal places
- Works for any number of days

**Acceptance Tests:**
- Given $50/day rate and 5 days, price = $250.00
- Given $50/day rate and 1 day, price = $50.00
- Given $50/day rate and 10 days, price = $500.00

**Complexity**: 2 points

---

### User Story 3.2: Calculate Rental Price (Business)
**As a** system  
**I want to** calculate rental price for business cars  
**So that** customers get bulk discount

**Acceptance Criteria:**
- Business pricing: 5% discount for 7+ days
- Price = DailyRate × NumberOfDays × DiscountFactor
- DiscountFactor = 1.0 for 1-6 days, 0.95 for 7+ days
- Price rounded to 2 decimal places

**Acceptance Tests:**
- Given $100/day rate and 6 days, price = $600.00 (no discount)
- Given $100/day rate and 7 days, price = $665.00 (5% discount)
- Given $100/day rate and 8 days, price = $760.00 (5% discount)

**Complexity**: 3 points

---

### User Story 3.3: Calculate Rental Price (Premium)
**As a** system  
**I want to** apply progressive discounts for premium car rentals  
**So that** long-term customers save significantly

**Acceptance Criteria:**
- 0% discount: 1-3 days
- 5% discount: 4-7 days
- 10% discount: 8-14 days
- 15% discount: 15+ days
- Price formula: DailyRate × Days × (1 - DiscountPercentage)

**Acceptance Tests:**
- Given $200/day and 3 days, price = $600 (0%)
- Given $200/day and 5 days, price = $950 (5%)
- Given $200/day and 10 days, price = $1800 (10%)
- Given $200/day and 15 days, price = $2550 (15%)

**Complexity**: 3 points

---

### User Story 3.4: Process Payment
**As a** system  
**I want to** record and track rental payments  
**So that** we can manage accounting

**Acceptance Criteria:**
- Payment created for each rental
- Initial status: Pending
- Payment can transition to Paid (with transaction reference)
- Payment can transition to Failed
- Amount matches rental total price
- Cannot change paid payment status

**Acceptance Tests:**
- Given new rental, when checking payments, then one payment created with Pending status
- Given pending payment, when marking as paid, then status changes to Paid, reference recorded
- Given paid payment, when trying to mark as failed, then error returned

**Complexity**: 3 points

---

## Epic 4: Customers & Data

### User Story 4.1: Manage Customers
**As a** rental agent  
**I want to** maintain customer records  
**So that** I can track customer information

**Acceptance Criteria:**
- Store customer name, email, phone number
- Validate email format (contains @)
- Store driver license number (required)
- Track creation date
- Can retrieve customer by email

**Acceptance Tests:**
- Given valid customer data, when creating customer, then stored successfully
- Given invalid email (no @), when creating customer, then error
- Given empty name, when creating customer, then error

**Complexity**: 2 points

---

## Epic 5: Domain Model & Business Rules

### User Story 5.1: Enforce State Machines
**As a** system  
**I want to** enforce valid state transitions for rentals  
**So that** business logic remains consistent

**Acceptance Criteria:**
- Rental states: Created → Active → Completed/Cancelled
- Cannot skip states (no Created → Completed)
- Cannot revert states
- Cannot transition from Completed
- Each transition has business rule

**Acceptance Tests:**
- Given Created rental, when activating, then transitions to Active
- Given Created rental, when cancelling, then transitions to Cancelled
- Given Active rental, when completing, then transitions to Completed
- Given Completed rental, when trying to complete, then error

**Complexity**: 4 points

---

### User Story 5.2: Value Object - DateRange
**As a** system  
**I want to** validate and work with date ranges  
**So that** rental period validation is consistent

**Acceptance Criteria:**
- Start date must be before end date
- Cannot be same date
- Can check overlap with other ranges
- Can calculate inclusive day count
- Immutable after creation

**Acceptance Tests:**
- Given dates 1/1 - 1/5, can create DateRange, days = 5
- Given dates 1/5 - 1/1, then error on creation
- Given two overlapping ranges, overlap check returns true
- Given non-overlapping ranges, overlap check returns false

**Complexity**: 3 points

---

### User Story 5.3: Value Object - Money
**As a** system  
**I want to** work safely with monetary values  
**So that** pricing calculations are type-safe

**Acceptance Criteria:**
- Cannot create negative amounts
- Can add amounts (same currency)
- Can multiply by days count
- Tracks currency
- Immutable after creation

**Acceptance Tests:**
- Given positive amount, Money created successfully
- Given negative amount, then error
- Given $50 + $75, result = $125
- Given $50 × 5 days, result = $250

**Complexity**: 2 points

---

## Epic 6: Architecture & Patterns

### User Story 6.1: Repository Pattern Implementation
**As a** developer  
**I want to** use repositories for data access  
**So that** I can swap implementations (JSON, SQL, etc.)

**Acceptance Criteria:**
- Repository interfaces abstract data access
- Implementations don't leak persistence details
- Can load and save to JSON files
- Thread-safe operations

**Complexity**: 4 points

---

### User Story 6.2: Strategy Pattern - Pricing
**As a** system  
**I want to** support pluggable pricing strategies  
**So that** new categories can be added easily

**Acceptance Criteria:**
- IPricingStrategy interface defined
- Three implementations: Economy, Business, Premium
- Easy to add new strategies without modifying existing code
- Strategy selected by car category

**Complexity**: 3 points

---

### User Story 6.3: Dependency Injection
**As a** developer  
**I want to** use DI container for object creation  
**So that** dependencies are managed centrally

**Acceptance Criteria:**
- DI container setup in Main
- All repositories registered
- Services registered as scoped
- Wiring verified in tests

**Complexity**: 3 points

---

## Epic 7: Testing

### User Story 7.1: Unit Tests for Domain
**As a** developer  
**I want to** write unit tests for entities  
**So that** business logic is verified

**Acceptance Criteria:**
- Test each entity's validation
- Test state transitions
- Test value object operations
- Test business rule enforcement
- Coverage > 80%

**Complexity**: 5 points

---

### User Story 7.2: Integration Tests
**As a** developer  
**I want to** test complete workflows  
**So that** end-to-end functionality verified

**Acceptance Criteria:**
- Test full rental creation to completion
- Test reservation to rental conversion
- Test pricing calculation end-to-end
- Test conflict detection

**Complexity**: 5 points

---

## Backlog Prioritization

### Critical Path (Must Complete)
1. User Story 1.1: View Available Cars
2. User Story 1.2: Start a Rental
3. User Story 1.3: Complete a Rental
4. User Story 3.1-3.3: Pricing Calculations
5. User Story 7.1: Unit Tests

### High Priority (Should Complete)
1. User Story 1.4: No Double Booking
2. User Story 2.1: Create Reservation
3. User Story 6.1-6.3: Architecture Patterns
4. User Story 7.2: Integration Tests

### Medium Priority (Nice to Have)
1. User Story 2.2: Cancel Reservation
2. User Story 3.4: Payment Processing
3. User Story 5.1-5.3: Domain Model Tests

### Release Planning

**Release 1.0 (MVP)**
- Epics 1-3 (rental, reservation, pricing)
- All critical path stories
- 80% test coverage
- Basic documentation

**Release 1.1 (Quality)**
- Additional test cases
- Error handling improvements
- Documentation expansion

**Release 2.0 (Future)**
- Database layer
- REST API
- Web UI
- Advanced analytics

---

**Backlog Version**: 1.0
**Last Updated**: April 2026
**Total Points**: ~85 points
**Estimated Effort**: 240 hours
