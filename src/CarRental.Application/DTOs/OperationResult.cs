namespace CarRental.Application.DTOs;

/// <summary>
/// Response DTO for successful operations.
/// </summary>
public class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }

    public static OperationResult CreateSuccess(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static OperationResult CreateFailure(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>
/// Generic response DTO for returning typed data.
/// </summary>
public class OperationResult<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static OperationResult<T> CreateSuccess(string message, T? data) =>
        new() { Success = true, Message = message, Data = data };

    public static OperationResult<T> CreateSuccess(string message) =>
        new() { Success = true, Message = message, Data = default };

    public static OperationResult<T> CreateFailure(string message) =>
        new() { Success = false, Message = message };
}
