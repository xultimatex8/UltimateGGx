namespace backend.Exceptions;

public class NotFoundException(string entityName, string propertyName, object value) : Exception($"{entityName} with {propertyName} '{value}' not found.")
{
    public string EntityName { get; } = entityName;
    public string PropertyName { get; } = propertyName;
    public object Value { get; } = value;
}