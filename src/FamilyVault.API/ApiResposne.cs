namespace FamilyVault.API;

public class ApiResposne<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public T? Data { get; set; }    

    public ApiResposne<T> Success(T data, string? message = null)
    {
        return new ApiResposne<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }
    
    public ApiResposne<T> Failure(string? message = null)
    {
        return new ApiResposne<T>
        {
            IsSuccess = false,
            Message = message
        };
    }
}
