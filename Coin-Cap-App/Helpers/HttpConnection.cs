using System.Text.Json;
using System.Text;

namespace Coin_Cap_App.Helpers
{
    public interface IHttpConnection
    {
        Task<T> WebRequest<T>(string url, string requestType, object? requestBody = null, Dictionary<string, string>? headers = null, string? authUser = null, string? authPword = null) where T : new();
    }

    public class HttpConnection : IHttpConnection
    {
        public async Task<T> WebRequest<T>(string url, string requestType, object? requestBody = null, Dictionary<string, string>? headers = null, string? authUser = null, string? authPword = null) where T : new()
        {
            T result = new();
            try
            {
                using var client = new HttpClient();
                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = requestType.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get
                };

                if (requestType.ToLower() == "post" && requestBody != null)
                {
                    var serializePayload = JsonSerializer.Serialize(requestBody);
                    requestMessage.Content = new StringContent(serializePayload, encoding: Encoding.UTF8, "application/json");
                }
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        requestMessage.Headers.Add(item.Key, item.Value);
                    }
                }
                if (!string.IsNullOrEmpty(authUser) && !string.IsNullOrEmpty(authPword))
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{authUser}:{authPword}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }



                var response = await client.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result = JsonSerializer.Deserialize<T>(responseContent);
                    var codeProperty = typeof(T).GetProperty("Code");
                    codeProperty.SetValue(result, "00");
                    var descriptionProperty = typeof(T).GetProperty("Description");
                    descriptionProperty.SetValue(result, "Successful");
                }
                else
                {
                    var errorResponse = new
                    {
                        code = "26",
                        description = responseContent
                    };
                    result = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(errorResponse));
                }
            }
            catch (Exception ex)
            {

                var errorResponse = new
                {
                    code = "99",
                    description = ex.Message
                };
                result = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(errorResponse));
            }
            return result;
        }
    }
}
