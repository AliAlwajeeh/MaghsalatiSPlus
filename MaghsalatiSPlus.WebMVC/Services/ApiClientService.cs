using MaghsalatiSPlus.WebMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MaghsalatiSPlus.WebMVC.Services
{
    public class LoginResultDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? ShopOwnerId { get; set; }
        public string? ShopName { get; set; }
    }

    public class ApiIdentityError
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
        public List<ApiIdentityError>? Errors { get; set; }
        public Dictionary<string, string[]>? ErrorsDictionary { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }
    }

    public class ApiClientService
    {
        private readonly string _defaultOwnerId;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseApiUrl;
        private string? _jwtToken;

        public ApiClientService(IHttpClientFactory cf, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _clientFactory = cf;
            _httpContextAccessor = httpContextAccessor;
            _baseApiUrl = (config["ApiSettings:BaseUrl"] ?? "").TrimEnd('/');
            _defaultOwnerId = config["AppUi:ShopOwnerId"] ?? string.Empty;
        }

        public void SetAuthToken(string? token) => _jwtToken = token;

        private HttpClient CreateClient()
        {
            var client = _clientFactory.CreateClient("API");

            if (!string.IsNullOrWhiteSpace(_jwtToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            if (client.DefaultRequestHeaders.Authorization is null)
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AuthToken")?.Value;
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private string Url(string path) => $"{_baseApiUrl}/{path.TrimStart('/')}";

   
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var client = CreateClient();
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Email ?? string.Empty), "Email");
            content.Add(new StringContent(model.Password ?? string.Empty), "Password");
            content.Add(new StringContent(model.ShopName ?? string.Empty), "ShopName");
            content.Add(new StringContent(model.PhoneNumber ?? string.Empty), "PhoneNumber");

            if (!string.IsNullOrWhiteSpace(model.Location))
                content.Add(new StringContent(model.Location), "Location");

            var confirmProp = typeof(RegisterViewModel).GetProperty("ConfirmPassword");
            if (confirmProp != null)
            {
                var cp = confirmProp.GetValue(model)?.ToString();
                if (!string.IsNullOrWhiteSpace(cp))
                    content.Add(new StringContent(cp!), "ConfirmPassword");
            }

            if (model.ProfileImageFile != null)
            {
                var fileContent = new StreamContent(model.ProfileImageFile.OpenReadStream());
                var mediaType = string.IsNullOrWhiteSpace(model.ProfileImageFile.ContentType)
                    ? "application/octet-stream"
                    : model.ProfileImageFile.ContentType;
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                content.Add(fileContent, "ProfileImageFile", model.ProfileImageFile.FileName);
            }

            var response = await client.PostAsync(Url("/api/Auth/register"), content);
            if (response.IsSuccessStatusCode)
                return (true, "تم إنشاء الحساب بنجاح.");

            var raw = await response.Content.ReadAsStringAsync();
            var parsed = ParseErrorMessage(raw);
            return (false, parsed ?? raw);
        }

        public async Task<LoginResultDto?> LoginAsync(LoginDto model)
        {
            var client = CreateClient();
            var payload = new { Email = model.Email, Password = model.Password };
            var response = await client.PostAsJsonAsync(Url("/api/Auth/login"), payload);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var token = root.TryGetProperty("token", out var tEl) ? tEl.GetString() : null;
            DateTime expiration = default;
            if (root.TryGetProperty("expiration", out var eEl) && eEl.ValueKind == JsonValueKind.String)
                DateTime.TryParse(eEl.GetString(), out expiration);

            if (string.IsNullOrWhiteSpace(token) || expiration == default)
                return null;

            _jwtToken = token!;
            var (ownerId, shopName) = ExtractFromJwt(token!);

            return new LoginResultDto
            {
                Token = token!,
                Expiration = expiration,
                ShopOwnerId = ownerId,
                ShopName = shopName
            };
        }

        private (string? OwnerId, string? ShopName) ExtractFromJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                string? ownerId = jwt.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid" || c.Type == "sub")?.Value;
                string? shopName = jwt.Claims.FirstOrDefault(c => c.Type == "shopName")?.Value;
                return (ownerId, shopName);
            }
            catch { return (null, null); }
        }

    
        public async Task<List<CustomerViewModel>?> GetCustomersByOwnerAsync(string? shopOwnerId = null)
        {
            shopOwnerId ??= _defaultOwnerId;
            var client = CreateClient();

            try
            {
                var all = await client.GetFromJsonAsync<List<CustomerViewModel>>(Url($"/api/Customers?shopOwnerId={shopOwnerId}"))
                          ?? new List<CustomerViewModel>();
                return all;
            }
            catch { return new List<CustomerViewModel>(); }
        }

        public async Task<bool> CreateCustomerAsync(CreateCustomerDto model)
        {
            model.ShopOwnerId ??= _defaultOwnerId;
            var client = CreateClient();
            var response = await client.PostAsJsonAsync(Url("/api/Customers"), model);
            return response.IsSuccessStatusCode;
        }

        private string? ResolveOwnerId(string? ownerId)
        {
            if (!string.IsNullOrWhiteSpace(ownerId)) return ownerId;
            if (!string.IsNullOrWhiteSpace(_defaultOwnerId)) return _defaultOwnerId;

            var user = _httpContextAccessor.HttpContext?.User;
            var fromClaims =
                user?.FindFirst("nameid")?.Value ??
                user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user?.FindFirst("sub")?.Value;

            return fromClaims;
        }


        public async Task<IEnumerable<CategoryViewModel>> GetCategoriesByOwnerAsync(string? ownerId = null)
        {
            var resolvedOwner = ResolveOwnerId(ownerId);
            var client = CreateClient();

            var url = string.IsNullOrWhiteSpace(resolvedOwner)
                ? Url("/api/categories")
                : Url($"/api/categories?shopOwnerId={resolvedOwner}");

            try
            {
                return await client.GetFromJsonAsync<List<CategoryViewModel>>(url)
                       ?? new List<CategoryViewModel>();
            }
            catch
            {
                return new List<CategoryViewModel>();
            }
        }


        public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id, string? ownerId = null)
        {
            var resolvedOwner = ResolveOwnerId(ownerId);
            var client = CreateClient();

            var url = string.IsNullOrWhiteSpace(resolvedOwner)
                ? Url($"/api/categories/{id}")
                : Url($"/api/categories/{id}?shopOwnerId={resolvedOwner}");

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
        }

        
        public async Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(CategoryViewModel model)
        {
            model.ShopOwnerId ??= _defaultOwnerId;
            var client = CreateClient();
            var response = await client.PostAsJsonAsync(Url("/api/categories"), model);
            if (response.IsSuccessStatusCode) return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(error) ? null : error);
        }

        
        public async Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(CategoryViewModel model)
        {
            model.ShopOwnerId ??= _defaultOwnerId;
            var client = CreateClient();
            var response = await client.PutAsJsonAsync(Url($"/api/categories/{model.Id}"), model);
            if (response.IsSuccessStatusCode) return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(error) ? null : error);
        }

     
        public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id, string? ownerId = null)
        {
            var resolvedOwner = ResolveOwnerId(ownerId);
            var client = CreateClient();

            var url = string.IsNullOrWhiteSpace(resolvedOwner)
                ? Url($"/api/categories/{id}")
                : Url($"/api/categories/{id}?shopOwnerId={resolvedOwner}");

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode) return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(error) ? null : error);
        }



        public async Task<List<OrderViewModel>?> GetOrdersByOwnerAsync(string? shopOwnerId = null)
        {
            shopOwnerId ??= _defaultOwnerId;
            var client = CreateClient();
            var response = await client.GetAsync(Url($"/api/Orders?shopOwnerId={shopOwnerId}"));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                
                return null;
            }

            var orders = JsonSerializer.Deserialize<List<OrderViewModel>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return orders?.ToList();
        }

        public async Task<(bool Success, string ErrorMessage)> CreateOrderAsync(CreateOrderViewModel model)
        {
            var client = CreateClient();
            using var content = new MultipartFormDataContent();

            
            content.Add(new StringContent(model.CustomerId.ToString()), "CustomerId");

            
            var items = (model.OrderItems ?? new List<CreateOrderItemViewModel>())
                .Select(oi => new
                {
                    itemName = oi.ItemName,
                    quantity = oi.Quantity,
                    service = oi.Service,
                    price = oi.Price,
                    categoryId = oi.CategoryId
                }).ToList();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var itemsJson = JsonSerializer.Serialize(items, jsonOptions);
            content.Add(new StringContent(itemsJson, System.Text.Encoding.UTF8, "application/json"), "OrderItemsJson");

         
            var imageFiles = model.ItemImages?.Where(f => f != null && f.Length > 0).ToList() ?? new List<IFormFile>();
            if (imageFiles.Any())
            {
                foreach (var imageFile in imageFiles)
                {
                    var stream = new StreamContent(imageFile.OpenReadStream());
                    var mediaType = string.IsNullOrWhiteSpace(imageFile.ContentType)
                        ? "application/octet-stream" : imageFile.ContentType;
                    stream.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

                    content.Add(stream, "ItemImages", imageFile.FileName);
                }
            }

            var response = await client.PostAsync(Url("/api/Orders"), content);

            if (response.IsSuccessStatusCode) return (true, string.Empty);

            var raw = await response.Content.ReadAsStringAsync();
            var parsed = ParseErrorMessage(raw);
            return (false, parsed ?? raw);
        }
     
        public async Task<CustomerViewModel?> GetCustomerByIdAsync(int id)
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/customers/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerViewModel>();
        }

    
        public async Task<bool> UpdateCustomerAsync(int id, CreateCustomerDto model)
        {
           
            model.ShopOwnerId ??= _defaultOwnerId;

            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"api/customers/{id}", model);
            return response.IsSuccessStatusCode;
        }

        
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/customers/{id}");
            return response.IsSuccessStatusCode;
        }

       
        private string? ParseErrorMessage(string json)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(json, options);
                if (apiError != null)
                {
                  
                    if (apiError.Errors != null && apiError.Errors.Any())
                    {
                        var messages = apiError.Errors
                            .Select(e => string.IsNullOrWhiteSpace(e.Description) ? e.Code : e.Description)
                            .Where(s => !string.IsNullOrWhiteSpace(s));
                        var joined = string.Join(" | ", messages);
                        if (!string.IsNullOrWhiteSpace(joined)) return joined;
                    }

                    
                    if (apiError.ErrorsDictionary != null && apiError.ErrorsDictionary.Any())
                    {
                        var messages = apiError.ErrorsDictionary
                            .SelectMany(kv => kv.Value ?? Array.Empty<string>())
                            .Where(s => !string.IsNullOrWhiteSpace(s));
                        var joined = string.Join(" | ", messages);
                        if (!string.IsNullOrWhiteSpace(joined)) return joined;
                    }

                
                    if (!string.IsNullOrWhiteSpace(apiError.Message))
                        return apiError.Message;

                    var td = $"{apiError.Title} {apiError.Detail}".Trim();
                    if (!string.IsNullOrWhiteSpace(td)) return td;
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errs))
                {
                    if (errs.ValueKind == JsonValueKind.Object)
                    {
                        var msgs = errs.EnumerateObject()
                            .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                            .Where(s => !string.IsNullOrWhiteSpace(s));
                        var joined = string.Join(" | ", msgs);
                        if (!string.IsNullOrWhiteSpace(joined)) return joined;
                    }
                }

                string? title = root.TryGetProperty("title", out var t) ? t.GetString() : null;
                string? detail = root.TryGetProperty("detail", out var d) ? d.GetString() : null;
                var combined = $"{title} {detail}".Trim();
                if (!string.IsNullOrWhiteSpace(combined)) return combined;
            }
            catch
            {
             
            }
            return null;
        }
        public async Task<OrderViewModel?> GetOrderByIdAsync(int id)
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/orders/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<OrderViewModel>();
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderAsync(UpdateOrderViewModel model)
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"api/orders/{model.Id}", model);
            if (response.IsSuccessStatusCode) return (true, null);
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderAsync(int id)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/orders/{id}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

    }


}

