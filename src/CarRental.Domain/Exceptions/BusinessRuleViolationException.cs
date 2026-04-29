namespace КаталогАвтоОрнда.Domain.Exceptions;

public class ПорушенняБізнесПравил : Exception
{
    public ПорушенняБізнесПравил(string повідомлення) : base(повідомлення) { }
}
