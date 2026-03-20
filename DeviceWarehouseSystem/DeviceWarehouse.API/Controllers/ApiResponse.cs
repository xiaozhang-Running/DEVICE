namespace DeviceWarehouse.API.Controllers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    private ApiResponse(bool success, string? message, T? data)
    {
        this.Success = success;
        this.Message = message;
        this.Data = data;
    }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>(true, message, data);
    }

    public static ApiResponse<T> ErrorResponse(string message)
    {
        return new ApiResponse<T>(false, message, default);
    }
}