# Feature-Based Services Architecture

This document describes the feature-based organization of services in the Application layer.

## Structure Overview

```
Application/
└── Services/
    ├── Auth/                    # Authentication feature services
    │   ├── Login/              # Login-specific services (if needed)
    │   ├── Register/           # Registration services (future)
    │   └── Token/              # Token management services (future)
    │
    ├── User/                    # User management feature services
    │   ├── Profile/            # User profile services
    │   │   ├── IUserProfileService.cs
    │   │   └── UserProfileService.cs
    │   └── Settings/           # User settings services (future)
    │
    ├── Dashboard/               # Dashboard feature services
    │   ├── Interfaces/
    │   │   └── IUserDashboardService.cs
    │   └── UserDashboardService.cs
    │
    └── Common/                  # Cross-cutting services
        └── DateTimeService.cs  # Common date/time operations
```

## Principles

### 1. Feature-Based Organization
- Services are organized by **business domain/feature**, not by technical concern
- Each feature has its own folder with related services
- Related services are grouped together (e.g., Profile services under User/Profile)

### 2. Separation of Concerns
- **Application Services**: Business logic and orchestration (Application layer)
- **Infrastructure Services**: Technical implementations (Infrastructure layer)
  - Security (JWT, Password Hashing)
  - External APIs
  - Data Access (Repositories)

### 3. Dependency Rules
- Feature services can depend on:
  - Domain entities
  - Other feature services (when necessary)
  - Infrastructure interfaces (via Application interfaces)
- Feature services **should NOT** depend on:
  - Other feature services directly (use interfaces)
  - Infrastructure implementations
  - Framework-specific code

## Service Categories

### Feature Services (Application Layer)

#### Dashboard Services
**Location**: `Application/Services/Dashboard/`

- **IUserDashboardService**: Aggregates dashboard data
- **UserDashboardService**: Implementation that orchestrates profile, summary, and activities

**Usage**:
```csharp
// In CQRS Query Handler
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, UserDashboardViewModel>
{
    private readonly IUserDashboardService _dashboardService;
    
    public async Task<UserDashboardViewModel> Handle(...)
    {
        return await _dashboardService.GetDashboardDataAsync(userId);
    }
}
```

#### User Profile Services
**Location**: `Application/Services/User/Profile/`

- **IUserProfileService**: User profile operations
- **UserProfileService**: Manages user profiles, account summaries, and activities

**Responsibilities**:
- Get/Update user profile
- Get account summary
- Manage recent activities
- Track user actions

#### Common Services
**Location**: `Application/Services/Common/`

- **IDateTimeService**: Date/time operations (abstraction for testability)
- **DateTimeService**: Implementation

**Purpose**: Cross-cutting concerns that are used by multiple features.

### Infrastructure Services (Infrastructure Layer)

These remain organized by technical concern:

```
Infrastructure/
├── Security/              # Security-related services
│   ├── PasswordHasher.cs
│   └── JwtTokenService.cs
├── ExternalServices/      # External API integrations
│   └── PriceService.cs
└── Persistence/          # Data access
    └── Repositories/
        └── UserRepository.cs
```

## Service Registration

All services are registered in `Program.cs`:

```csharp
// Application - Services (Feature-based organization)
builder.Services.AddScoped<Application.Services.User.Profile.IUserProfileService, 
    Application.Services.User.Profile.UserProfileService>();
builder.Services.AddScoped<Application.Services.Dashboard.Interfaces.IUserDashboardService, 
    Application.Services.Dashboard.UserDashboardService>();
builder.Services.AddScoped<Application.Services.Common.IDateTimeService, 
    Application.Services.Common.DateTimeService>();

// Infrastructure - Services (Technical concerns)
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPriceService, PriceService>();
```

## Adding New Features

### Example: Adding a "Notifications" Feature

1. **Create feature folder structure**:
```
Application/Services/Notifications/
├── Interfaces/
│   └── INotificationService.cs
└── NotificationService.cs
```

2. **Define interface**:
```csharp
namespace GoldPriceTracker.Application.Services.Notifications.Interfaces;

public interface INotificationService
{
    Task<List<Notification>> GetNotificationsAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
}
```

3. **Implement service**:
```csharp
namespace GoldPriceTracker.Application.Services.Notifications;

public class NotificationService : INotificationService
{
    // Implementation
}
```

4. **Register in Program.cs**:
```csharp
builder.Services.AddScoped<Application.Services.Notifications.Interfaces.INotificationService, 
    Application.Services.Notifications.NotificationService>();
```

## Benefits

### ✅ Scalability
- Easy to add new features without cluttering
- Clear boundaries between features
- Services are easy to locate

### ✅ Maintainability
- Related services are grouped together
- Changes to one feature don't affect others
- Clear ownership and responsibility

### ✅ Testability
- Services can be easily mocked
- Feature boundaries make testing easier
- Dependencies are explicit

### ✅ Clean Architecture Compliance
- Application layer contains business logic
- Infrastructure layer contains technical implementations
- Clear separation of concerns

## Migration from Old Structure

### Old Structure (Flat)
```
Services/
├── IUserService.cs
├── UserService.cs
├── IUserProfileService.cs
├── UserProfileService.cs
├── IUserDashboardService.cs
└── UserDashboardService.cs
```

### New Structure (Feature-Based)
```
Application/Services/
├── User/
│   └── Profile/
│       ├── IUserProfileService.cs
│       └── UserProfileService.cs
└── Dashboard/
    ├── Interfaces/
    │   └── IUserDashboardService.cs
    └── UserDashboardService.cs
```

**Note**: Old services in `Services/` folder are deprecated and should be removed after migration is complete.

## Best Practices

1. **One Feature, One Folder**: Each business feature gets its own folder
2. **Interfaces First**: Define interfaces before implementations
3. **Dependency Injection**: Always use DI, never instantiate directly
4. **Single Responsibility**: Each service should have one clear purpose
5. **Feature Independence**: Features should not directly depend on each other
6. **Common Services**: Only use Common folder for truly cross-cutting concerns

## Future Enhancements

Potential new feature services:
- `Application/Services/Auth/Register/` - User registration
- `Application/Services/Auth/Token/` - Token refresh, revocation
- `Application/Services/User/Settings/` - User preferences
- `Application/Services/Notifications/` - Notification management
- `Application/Services/Reports/` - Reporting services
- `Application/Services/Analytics/` - Analytics services

