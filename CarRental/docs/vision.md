# Car Rental System

## Problem
Build a layered car rental application that prevents double-bookings via date-conflict detection and persists bookings to JSON.

## Core Use Cases
1. **List Available Cars** - Display cars with category, model, and availability status
2. **Create Booking** - Validate car availability, check date overlaps, calculate pricing by category (Economy/Standard/SUV)
3. **Cancel Booking** - Release car after cancellation, persist state to disk

## Functional Requirements
- Multi-layered architecture: Domain (entities, validators) → Application (services, DTO) → Infrastructure (JSON repos)
- Date-conflict detection: Block overlapping bookings on same vehicle
- Pricing strategies per vehicle category using Strategy pattern
- Async I/O for file persistence (LoadAsync/SaveAsync)

## Non-Functional Requirements
- Minimal 20 unit tests + 8 integration tests (xUnit with AAA pattern)
- Fault-tolerance: Handle missing JSON, corrupted data, file access errors
- Clean Code: No XML summary comments, encapsulate business logic in Domain layer
- Persistence: All state changes written to `cars.json` and `bookings.json`

## Constraints (Iteration 1)
- Console UI only (no web/API)
- No authentication, payment, or tiered pricing
- Single-threaded booking flow
- Local file storage (no database)
