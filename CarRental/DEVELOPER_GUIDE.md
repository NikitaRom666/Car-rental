# Довідник для розробника

## Архітектура
- `Domain` містить сутності, value objects, enum-и та контракти.
- `Application` містить use cases та оркестрацію.
- `Infrastructure` реалізує файлову персистенцію.
- `ConsoleUI` забезпечує взаємодію з користувачем.
- `Tests` містить xUnit-тести.

## Збірка і тести
```powershell
dotnet build
dotnet test
```

## Примітки
- Бізнес-правила слід тримати на рівні `Application` і `Domain`.
- Не переносити валідацію лише в консольний шар.
- Зберігати шляхи до JSON-файлів у `Infrastructure`.
