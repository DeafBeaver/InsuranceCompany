namespace InsuranceSystem.Core.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, int id)
            : base($"{entityName} з ID {id} не знайдено!") { }
    }

    public class ValidationException : DomainException
    {
        public ValidationException(string message) : base(message) { }
    }
}
