using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChefKnifeStudios.ExpenseTracker.WebAPI.Models;

namespace ChefKnifeStudios.ExpenseTracker.WebAPI;

public interface IHttpService
{
    Task<ApiResponse<T?>> GetAsync<T>(string url);
    Task<ApiResponse<Y?>> PostAsync<X, Y>(string url, X data);
    Task<ApiResponse<Y?>> PostAsync<Y>(string url, FormUrlEncodedContent data);
    Task<ApiResponse<Y?>> PutAsync<X, Y>(string url, X data);
    Task<ApiResponse<T?>> DeleteAsync<T>(string url);
    void SetBasicAuthentication(string username, string password);
    void SetBearerAuthentication(string token);
    void AddHeader(string key, string value);
}

public class HttpService : IHttpService
{
    readonly IHttpClientFactory _httpClientFactory;
    string? _basicAuthHeaderValue;
    string? _bearerAuthHeaderValue;
    Dictionary<string, string> _additionalHeaders = new();

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse<T?>> GetAsync<T>(string url)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorCodeResponse<T>(response);
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<T?>(content, GetJsonSerializerOptions());

            return new ApiResponse<T?>(obj);
        }
        catch (HttpRequestException httpEx)
        {
            return CreateErrorCodeResponse<T>(httpEx);
        }
        catch (Exception ex)
        {
            return CreateFallbackResponse<T>(ex);
        }
    }

    public async Task<ApiResponse<Y?>> PostAsync<X, Y>(string url, X data)
    {
        try
        {
            var client = CreateClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(data, GetJsonSerializerOptions()),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorCodeResponse<Y>(response);
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Y?>(content, GetJsonSerializerOptions());
            return new ApiResponse<Y?>(obj);
        }
        catch (Exception ex)
        {
            return CreateFallbackResponse<Y>(ex);
        }
    }

    public async Task<ApiResponse<Y?>> PostAsync<Y>(string url, FormUrlEncodedContent formContent)
    {
        try
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(url, formContent);

            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorCodeResponse<Y>(response);
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Y?>(content, GetJsonSerializerOptions());

            return new ApiResponse<Y?>(obj);
        }
        catch (Exception ex)
        {
            return CreateFallbackResponse<Y>(ex);
        }
    }

    public async Task<ApiResponse<Y?>> PutAsync<X, Y>(string url, X data)
    {
        try
        {
            var client = CreateClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(data, GetJsonSerializerOptions()),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorCodeResponse<Y>(response);
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Y?>(content, GetJsonSerializerOptions());
            return new ApiResponse<Y?>(obj);
        }
        catch (Exception ex)
        {
            return CreateFallbackResponse<Y>(ex);
        }
    }

    public async Task<ApiResponse<T?>> DeleteAsync<T>(string url)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return await CreateErrorCodeResponse<T>(response);
            }

            var content = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<T?>(content, GetJsonSerializerOptions());
            return new ApiResponse<T?>(obj);
        }
        catch (Exception ex)
        {
            return CreateFallbackResponse<T>(ex);
        }
    }

    public void SetBasicAuthentication(string username, string password)
    {
        _basicAuthHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    public void SetBearerAuthentication(string token)
    {
        _bearerAuthHeaderValue = token;
    }

    public void AddHeader(string key, string value)
    {
        _additionalHeaders[key] = value;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        if (_basicAuthHeaderValue != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _basicAuthHeaderValue);
        }

        if (_bearerAuthHeaderValue != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerAuthHeaderValue);
        }

        foreach (var header in _additionalHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }

    private async Task<ApiResponse<T?>> CreateErrorCodeResponse<T>(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        return new ApiResponse<T?>(
            responseMessage: $"Request failed with status code {(int)response.StatusCode}: {errorContent}",
            httpStatusCode: (int)response.StatusCode,
            responseException: new ApiException
            {
                Message = errorContent,
                Code = (int)response.StatusCode,
                StackTrace = null,
                InnerExcpetion = null
            });
    }

    private ApiResponse<T?> CreateErrorCodeResponse<T>(HttpRequestException httpEx)
    {
        return new ApiResponse<T?>(
            responseMessage: httpEx.Message,
            httpStatusCode: (int)(httpEx.Data["StatusCode"] ?? HttpStatusCode.InternalServerError),
            responseException: new ApiException
            {
                Message = httpEx.Message,
                Code = (int)(httpEx.Data["StatusCode"] ?? HttpStatusCode.InternalServerError),
                StackTrace = httpEx.StackTrace,
                InnerExcpetion = httpEx.InnerException?.ToString()
            });
    }

    private static ApiResponse<T?> CreateFallbackResponse<T>(Exception? ex = null)
    {
        return new ApiResponse<T?>
        {
            StatusCode = 500,
            Message = ex?.Message ?? string.Empty,
            IsSuccessful = false,
            Data = default,
            Exception = null,
        };
    }

    private JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
