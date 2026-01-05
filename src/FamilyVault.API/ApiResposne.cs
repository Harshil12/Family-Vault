public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }   // Useful for mapping business/domain errors
    public string? TraceId { get; set; }     // Correlation ID for logging/tracing
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null, string? traceId = null) 
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            TraceId = traceId
        };
    }

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