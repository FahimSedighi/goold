using FluentValidation;
using GoldPriceTracker.Application.Common.Behaviors;
using GoldPriceTracker.Infrastructure.ExternalServices;
using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using MediatR;
using System.Reflection;

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
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
