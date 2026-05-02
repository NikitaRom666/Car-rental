# Оренда автомобілів

Навчальний міні-проєкт про бронювання авто. Тут є шари для домену, бізнес-логіки, інфраструктури, консолі та тестів.

## Запуск
```powershell
dotnet run --project ConsoleUI/ConsoleUI.csproj
```

## Тестування
```powershell
dotnet test
```

## Структура
- `Domain` - сутності, enum-и, інтерфейси, валідація
- `Application` - сервіси, DTO, бізнес-правила, запити
- `Infrastructure` - файлові репозиторії та JSON-персистенція
- `ConsoleUI` - консольне меню
- `Tests` - unit та integration тести

## Документація
- `docs/vision.md`
- `docs/backlog.md`
- `docs/class-diagram.md`
- `docs/sequence-diagram.md`
- `docs/architecture-diagram.md`
- `docs/iteration-1.md`
- `docs/iteration-2-plan.md`
- `docs/iteration-2.md`
- `docs/test-strategy.md`
- `docs/test-matrix.md`
- `docs/iteration-3.md`
