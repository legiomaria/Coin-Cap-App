using Coin_Cap_App.Data;
using Coin_Cap_App.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Coin_Cap_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptoCurrencyController : ControllerBase
    {
        readonly IHttpConnection _httpConnection;
        string? url;
        public CryptoCurrencyController(IConfiguration configuration, IHttpConnection httpConnection)
        {
            _httpConnection = httpConnection;
            //https://api.coincap.io/v2/assets
            url = configuration.GetValue<string>("AppOptions:url");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5)
        {
            var apiResponse = await _httpConnection.WebRequest<ApiResponse>(url, "get");
            if (apiResponse.data.Any())
            {
                var totalCount = apiResponse.data.Count();
                var pagedData = apiResponse.data
                    .OrderBy(x => x.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paginatedResponse = new PaginatedResponse<CryptoField>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Data = pagedData
                };

                return Ok(paginatedResponse);
            }

            return Ok("No record found");
        }

        [HttpGet("by-rank")]
        public async Task<IActionResult> GetById([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("id value can not be null or empty");

            var response = await _httpConnection.WebRequest<ApiResponse>(url, "get");
            if (response.Code.Equals("00"))
            {
                return Ok(response.data.Where(x => x.id.ToLower().Equals(id.ToLower())).FirstOrDefault());
            }
            return BadRequest($"No reesponse for id = {id}. Please try another");
        }
    }
}
