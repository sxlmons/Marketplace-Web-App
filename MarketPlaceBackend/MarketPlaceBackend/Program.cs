using MarketPlaceBackend;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MarketPlaceBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Add services ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Logger
builder.Services.AddScoped<Logger>();

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None; // <-- allow cross-site cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // must use HTTPS
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// --- Enable CORS for React frontend ---

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCors", policy =>
    {
        policy.WithOrigins("http://localhost:56987") // your React dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // allows cookies/session
    });
});

var app = builder.Build();

// --- Middleware ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply CORS BEFORE authentication/authorization
app.UseCors("ReactCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
