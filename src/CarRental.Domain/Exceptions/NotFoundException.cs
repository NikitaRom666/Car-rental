namespace КаталогАвтоОрнда.Domain.Exceptions;

public class НеЗнайдено : Exception
{
    public НеЗнайдено(string повідомлення) : base(повідомлення) { }
    public НеЗнайдено(string типОб'єкту, Guid ід) 
        : base($"{}типОб'єкту} ід ‘{}ід:N}’ не знайден.") { }
}
