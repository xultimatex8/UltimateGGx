namespace backend.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, string propertyName, object value)
        : base($"{entityName} with {propertyName} '{value}' not found.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        Value = value;
    }

    public string? EntityName { get; }
    public string? PropertyName { get; }
    public object? Value { get; }
}