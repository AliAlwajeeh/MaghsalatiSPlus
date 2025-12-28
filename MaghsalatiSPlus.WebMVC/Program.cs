

using MaghsalatiSPlus.WebMVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddControllersWithViews();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/Error";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();


builder.Services.AddHttpContextAccessor();


var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing");


builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);               
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.Timeout = TimeSpan.FromSeconds(20);
});


builder.Services.AddScoped<ApiClientService>();


builder.Services.Configure<AppUiOptions>(builder.Configuration.GetSection("AppUi"));


builder.Services.AddHttpClient<CategoriesApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();



public class AppUiOptions
{
    
    public string? ShopOwnerId { get; set; }
}


public class CategoriesApiClient
{
    private readonly HttpClient _http;
    public CategoriesApiClient(HttpClient http) => _http = http;

    private static string Q(string ownerId) => $"?shopOwnerId={Uri.EscapeDataString(ownerId)}";

    public async Task<List<MaghsalatiSPlus.WebMVC.Models.CategoryViewModel>> GetAllAsync(string ownerId)
    {
        var res = await _http.GetAsync($"/api/Categories{Q(ownerId)}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<MaghsalatiSPlus.WebMVC.Models.CategoryViewModel>>() ?? new();
    }

    public async Task<MaghsalatiSPlus.WebMVC.Models.CategoryViewModel?> GetByIdAsync(string ownerId, int id)
    {
        var res = await _http.GetAsync($"/api/Categories/{id}{Q(ownerId)}");
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<MaghsalatiSPlus.WebMVC.Models.CategoryViewModel>();
    }

    public async Task<int> CreateAsync(MaghsalatiSPlus.WebMVC.Models.CategoryCreateUpdateDto dto)
    {
        var res = await _http.PostAsJsonAsync($"/api/Categories", dto);
        res.EnsureSuccessStatusCode();
        var created = await res.Content.ReadFromJsonAsync<MaghsalatiSPlus.WebMVC.Models.CategoryViewModel>();
        return created?.Id ?? 0;
    }

    public async Task UpdateAsync(int id, MaghsalatiSPlus.WebMVC.Models.CategoryCreateUpdateDto dto, string ownerId)
    {
        var res = await _http.PutAsJsonAsync($"/api/Categories/{id}{Q(ownerId)}", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string ownerId, int id)
    {
        var res = await _http.DeleteAsync($"/api/Categories/{id}{Q(ownerId)}");
        res.EnsureSuccessStatusCode();
    }
}
