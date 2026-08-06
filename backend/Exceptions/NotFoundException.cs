namespace backend.Exceptions;

public class NotFoundException : Exception
{
    public string UserMessage { get; }
    public string? EntityName { get; }
    public string? PropertyName { get; }
    public object? Value { get; }

    public NotFoundException(string userMessage)
        : base(userMessage)
    {
        UserMessage = userMessage;
    }

    public NotFoundException(
        string userMessage,
        string entityName,
        string propertyName,
        object value)
        : base($"{entityName} with {propertyName} '{value}' not found.")
    {
        UserMessage = userMessage;
        EntityName = entityName;
        PropertyName = propertyName;
        Value = value;
    }
}