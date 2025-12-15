using FluentValidation;
using GoldPriceTracker.Application.Common.Behaviors;
using GoldPriceTracker.Infrastructure.ExternalServices;
using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add MediatR for CQRS pattern
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add MediatR pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Application - Services (Feature-based organization)
builder.Services.AddScoped<GoldPriceTracker.Application.Services.Common.IDateTimeService, GoldPriceTracker.Application.Services.Common.DateTimeService>();

// Infrastructure - External Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddScoped<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

// JWT Authentication - Validate tokens issued by AuthService
// Note: This project does NOT issue tokens, only validates them
var jwtSecretKey = builder.Configuration["AuthService:JwtSecretKey"] ?? 
    builder.Configuration["Jwt:SecretKey"] ??
    throw new InvalidOperationException("JWT Secret Key must be configured in appsettings.json");
var jwtIssuer = builder.Configuration["AuthService:JwtIssuer"] ?? 
    builder.Configuration["Jwt:Issuer"] ?? 
    "AuthService";
var jwtAudience = builder.Configuration["AuthService:JwtAudience"] ?? 
    builder.Configuration["Jwt:Audience"] ?? 
    "AuthServiceUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Read token from cookie for MVC requests
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
