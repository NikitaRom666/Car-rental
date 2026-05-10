# Testing

## Run Tests
```bash
dotnet test
```

## Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Coverage

| Category | Unit Tests | Integration Tests |
|----------|------------|-------------------|
| Booking Creation | 5 | 2 |
| Booking Cancellation | 2 | 1 |
| Date Validation | 3 | - |
| Overlap Detection | 2 | 1 |
| Domain Entities | 8 | - |
| Strategy Pattern | 4 | 1 |
| LINQ Queries | 3 | 1 |
| Fault Handling | 5 | 2 |
| **Total** | **32** | **8** |

## Critical Scenarios
- Date overlap detection
- Car availability checks
- JSON persistence & reload
- Corrupted file handling (FileNotFoundException, UnauthorizedAccessException, JsonException)
- Domain invariants validation

## Quality Gate
- All tests must pass
- Coverage >= 70% for critical code paths
- No compile warnings
