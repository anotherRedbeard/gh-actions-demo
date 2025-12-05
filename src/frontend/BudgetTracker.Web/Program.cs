using BudgetTracker.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure API client - supports environment variable override
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
    ?? Environment.GetEnvironmentVariable("API_BASE_URL") 
    ?? "http://localhost:7071/api/";

builder.Services.AddHttpClient<BudgetApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Log the API base URL for debugging (don't log secrets in production!)
Console.WriteLine($"API Base URL configured: {apiBaseUrl}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
