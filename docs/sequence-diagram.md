# Архітектурне моделювання - Sequence Diagram
```mermaid
sequenceDiagram
    participant Console as UI Шар
    participant Service as Application Сервіс
    participant Domain as Domain (Car)
    participant Repo as Infrastructure Репозиторій
    
    Console->>Service: CreateBooking(carId, days)
    Service->>Repo: GetById(carId)
    Repo-->>Service: повертає об'єкт Car
    Service->>Domain: Rent() перевірка інваріантів
    Domain-->>Service: статус змінено
    Service->>Repo: SaveAsync() збереження стану
    Repo-->>Console: Успішне підтвердження
