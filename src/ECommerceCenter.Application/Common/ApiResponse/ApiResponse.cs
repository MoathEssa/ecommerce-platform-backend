using System.Net;

namespace ECommerceCenter.Application.Common.ApiResponse;

public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }
    public object? Meta { get; set; }
}
