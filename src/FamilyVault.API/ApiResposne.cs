/// <summary>
/// Represents ApiResponse.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets IsSuccess.
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Gets or sets Message.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// Gets or sets ErrorCode.
    /// </summary>
    public string? ErrorCode { get; set; }   // Useful for mapping business/domain errors
    /// <summary>
    /// Gets or sets TraceId.
    /// </summary>
    public string? TraceId { get; set; }     // Correlation ID for logging/tracing
    /// <summary>
    /// Gets or sets Data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Performs the Success operation.
    /// </summary>
    public static ApiResponse<T> Success(T? data, string? message = null, string? traceId = null) 
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            TraceId = traceId
        };
    }

    /// <summary>
    /// Performs the Failure operation.
    /// </summary>
    public static ApiResponse<T> Failure(string? message = null, string? errorCode = null, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            ErrorCode = errorCode,
            TraceId = traceId
        };
    }
}
