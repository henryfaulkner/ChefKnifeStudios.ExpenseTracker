namespace ChefKnifeStudios.ExpenseTracker.WebAPI.Models;

public class ApiResponse<T>
{
    public ApiResponse() { }

    public ApiResponse(
        T responseData, string responseMessage= "Successful", int httpStatusCode = 200)
    {
        StatusCode = httpStatusCode;
        Message = responseMessage;
        Data = responseData;
        IsSuccessful = httpStatusCode == 200;
    }

    public ApiResponse(string responseMessage,
        T responseData, int httpStatusCode = 200)
    {
        StatusCode = httpStatusCode;
        Message = responseMessage;
        Data = responseData;
        IsSuccessful = httpStatusCode == 200;
    }

    public ApiResponse(string responseMessage, int httpStatusCode, ApiException responseException)
    {
        StatusCode = httpStatusCode;
        Exception = responseException;
        IsSuccessful = false;
        Message = responseMessage;
    }

    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public bool IsSuccessful { get; set; }
    public T? Data { get; set; }
    public ApiException? Exception { get; set; }
}