# Архітектурне моделювання - Class Diagram
```mermaid
classDiagram
    class Car {
        +Guid Id
        +string Model
        +decimal BasePricePerDay
        +bool IsAvailable
        +Rent()
        +Return()
    }
    class Booking {
        +Guid Id
        +Guid CarId
        +DateTime StartDate
        +DateTime EndDate
        +decimal TotalPrice
    }
    interface ICarRepository {
        +GetById(id)
        +GetAll()
    }
    class JsonCarRepository {
        +LoadAsync()
        +SaveAsync()
    }
    ICarRepository <|.. JsonCarRepository
