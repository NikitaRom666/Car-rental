# Test Strategy

## Critical Scenarios
- Booking creation with availability check
- Date overlap detection for same vehicle
- Booking cancellation & car state reset
- JSON save/load/reload cycle
- Analytics queries after persistence

## Test Organization

**Unit Tests (Moq):**
- BookingService with mocked repositories
- Domain entity invariants
- Strategy pattern pricing
- LINQ query logic

**Integration Tests (Real I/O):**
- FileCarRepository: save/load cycle
- FileBookingRepository: persistence verification
- Fault scenarios: missing files, corrupted JSON, read-only files
- Use temp files with cleanup (Path.GetTempPath() + GUID)

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Corrupted JSON | PersistenceException with specific error type |
| Double cancellation | BookingStatus guards in Domain |
| File access denied | UnauthorizedAccessException handler |
| State inconsistency | Integration tests verify save/load |

## Quality Metrics
- 20+ unit tests (target: PASS)
- 8+ integration tests (target: PASS)
- Coverage: 70%+ critical paths
- CI gate: Build + Test + Coverage check