namespace AcademiaDoZe.Domain.Exceptions;

//thiago kovalski
public sealed class DomainExceptions(string message) : Exception(message)
{
}