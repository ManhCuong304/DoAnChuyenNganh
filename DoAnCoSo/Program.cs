using DoAnCoSo.Helpers;
using DoAnCoSo.Hubs;
using DoAnCoSo.Models;
using DoAnCoSo.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<MyAiService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddSignalR();

builder.Services.AddTransient<IEmailSender, StmpEmailSender>();
builder.Services.AddScoped<MessageService>();
var endpoint = builder.Configuration["AIService:Endpoint"];
var apiKey = builder.Configuration["AIService:ApiKey"];

if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
{
    throw new Exception("AIService configuration is missing Endpoint or ApiKey in appsettings.json");
}

builder.Services.AddHttpClient<PostService>(client =>
{
    client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");  
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["AIService:ApiKey"]}");
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// --- Đăng ký các service cần thiết ---
builder.Services.AddScoped<AIEmbeddingService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddHttpClient("NoProxy")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseProxy = false
    });

builder.Services.AddScoped<ProfileService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    AllowedOrigins = { "*" }
};
app.UseWebSockets(webSocketOptions);
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var messageService = context.RequestServices.GetRequiredService<MessageService>();

        if (messageService == null)
        {
            throw new Exception("MessageService is null!");
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("⚠️ User chưa đăng nhập khi mở WebSocket");
        }

        await WebSocketHandler.Handle(context, socket, messageService);
    }
    else
    {
        await next();
    }
});

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}");
app.Run();
