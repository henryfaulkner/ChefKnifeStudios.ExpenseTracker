using System.Net;

namespace ChefKnifeStudios.ExpenseTracker.Shared
{
    public class ApiResponse<T>
    {
        public ApiResponse() { }

        public ApiResponse(
            T responseData,
            HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            Data = responseData;
            HttpStatusCode = httpStatusCode;
        }

        public HttpStatusCode HttpStatusCode { get; set; } 
        public T? Data { get; set; }
    }
}
