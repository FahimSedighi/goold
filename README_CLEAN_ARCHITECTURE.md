# Clean Architecture & CQRS Implementation

This project follows **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)** principles.

## Project Structure

```
src/
├── Api/                          # Presentation Layer (Thin)
│   ├── Controllers/              # API Controllers (use MediatR)
│   ├── Middlewares/              # HTTP Middlewares
│   └── Filters/                  # Action Filters
│
├── Application/                  # Application Layer (Business Logic)
│   ├── Features/                 # Feature-based organization
│   │   ├── Auth/
│   │   │   └── Login/
│   │   │       ├── LoginCommand.cs      # Command (Write)
│   │   │       ├── LoginHandler.cs      # Command Handler
│   │   │       └── LoginValidator.cs    # FluentValidation
│   │   └── Dashboard/
│   │       └── Queries/
│   │           ├── GetDashboardDataQuery.cs
│   │           └── GetDashboardDataQueryHandler.cs
│   ├── Interfaces/               # Application Interfaces
│   │   └── Repositories/
│   │       └── IUserRepository.cs
│   └── Common/                   # Shared Application Code
│       ├── Behaviors/            # MediatR Pipeline Behaviors
│       ├── Exceptions/           # Custom Exceptions
│       └── Interfaces/           # ICommand, IQuery
│
├── Domain/                       # Domain Layer (Core Business Rules)
│   ├── Entities/                 # Domain Entities
│   │   ├── User.cs
│   │   └── PriceData.cs
│   ├── ValueObjects/             # Value Objects
│   │   └── Email.cs
│   ├── Events/                   # Domain Events
│   └── Enums/                    # Domain Enums
│
├── Infrastructure/               # Infrastructure Layer (External Concerns)
│   ├── Persistence/              # Data Access
│   │   └── Repositories/
│   │       └── UserRepository.cs
│   ├── Security/                 # Security Services
│   │   ├── Interfaces/
│   │   │   ├── IPasswordHasher.cs
│   │   │   └── IJwtTokenService.cs
│   │   ├── PasswordHasher.cs
│   │   └── JwtTokenService.cs
│   ├── ExternalServices/         # External API Integration
│   │   ├── Interfaces/
│   │   │   └── IPriceService.cs
│   │   └── PriceService.cs
│   └── Configurations/           # Infrastructure Configurations
│
└── Shared/                       # Shared Layer (Cross-cutting)
    └── Contracts/
        └── DTOs/                 # Data Transfer Objects
            ├── UserDto.cs
            ├── AuthResponse.cs
            ├── LoginRequest.cs
            ├── PriceViewModel.cs
            └── UserDashboardViewModel.cs
```

## Architecture Principles

### 1. Clean Architecture Layers

#### **Domain Layer** (Innermost)
- **No dependencies** on other layers
- Contains business entities and rules
- Pure C# classes, no framework dependencies

#### **Application Layer**
- Depends only on **Domain**
- Contains business logic and use cases
- Uses **CQRS** pattern (Commands/Queries)
- Interfaces defined here, implemented in Infrastructure

#### **Infrastructure Layer**
- Depends on **Application** and **Domain**
- Implements interfaces from Application layer
- Handles external concerns:
  - Database access
  - External APIs
  - Security (JWT, Password Hashing)
  - File system, Email, etc.

#### **Api Layer** (Outermost)
- Depends on **Application** and **Infrastructure**
- Thin controllers that delegate to MediatR
- No business logic in controllers
- Handles HTTP concerns only

### 2. CQRS Pattern

#### **Commands** (Write Operations)
```csharp
// Example: LoginCommand.cs
public class LoginCommand : ICommand<AuthResponse>
{
    public string EmailOrUsername { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
```

#### **Command Handlers** (Business Logic)
```csharp
// Example: LoginHandler.cs
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

#### **Queries** (Read Operations)
```csharp
// Example: GetDashboardDataQuery.cs
public class GetDashboardDataQuery : IQuery<UserDashboardViewModel>
{
    public int UserId { get; set; }
}
```

#### **Query Handlers**
```csharp
// Example: GetDashboardDataQueryHandler.cs
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, UserDashboardViewModel>
{
    public async Task<UserDashboardViewModel> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        // Read logic here
    }
}
```

### 3. Dependency Flow

```
Api → Application → Domain
  ↓        ↓
Infrastructure → Domain
```

**Rules:**
- ✅ Domain has **no dependencies**
- ✅ Application depends only on **Domain**
- ✅ Infrastructure depends on **Application** and **Domain**
- ✅ Api depends on **Application** and **Infrastructure**
- ❌ **Never** depend inward (Domain never depends on Application)

## Key Components

### MediatR (CQRS Implementation)
- **Commands**: `ICommand<TResponse>` → `IRequest<TResponse>`
- **Queries**: `IQuery<TResponse>` → `IRequest<TResponse>`
- **Handlers**: `IRequestHandler<TRequest, TResponse>`
- **Pipeline Behaviors**: Validation, Logging, etc.

### FluentValidation
- Validators for Commands/Queries
- Automatic validation via `ValidationBehavior`
- Throws `ValidationException` on failure

### Dependency Injection
All dependencies registered in `Program.cs`:
```csharp
// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Infrastructure Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

## Usage Examples

### Controller (Thin Layer)
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            EmailOrUsername = request.EmailOrUsername,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
```

### Command Handler (Business Logic)
```csharp
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user
        var user = await _userRepository.GetByEmailOrUsernameAsync(request.EmailOrUsername);
        
        // 2. Validate password
        var isValid = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);
        
        // 3. Generate token
        var token = _jwtTokenService.GenerateToken(user, request.RememberMe);
        
        // 4. Return response
        return new AuthResponse { Success = true, Token = token };
    }
}
```

## Benefits

### ✅ **Separation of Concerns**
- Each layer has a single responsibility
- Business logic isolated from infrastructure

### ✅ **Testability**
- Easy to mock interfaces
- Domain logic can be tested without infrastructure
- Handlers can be unit tested independently

### ✅ **Scalability**
- Easy to add new features (new Commands/Queries)
- Can swap infrastructure implementations
- Feature-based organization

### ✅ **Maintainability**
- Clear structure and dependencies
- Easy to understand and modify
- Changes isolated to specific layers

### ✅ **Flexibility**
- Can change database without affecting business logic
- Can change UI framework without affecting domain
- Easy to add new features

## Migration Notes

### Old Structure → New Structure

| Old Location | New Location |
|-------------|--------------|
| `Models/User.cs` | `Domain/Entities/User.cs` |
| `Services/UserService.cs` | `Infrastructure/Persistence/Repositories/UserRepository.cs` |
| `Services/AuthService.cs` | `Application/Features/Auth/Login/LoginHandler.cs` |
| `Services/JwtService.cs` | `Infrastructure/Security/JwtTokenService.cs` |
| `Services/PriceService.cs` | `Infrastructure/ExternalServices/PriceService.cs` |
| `Controllers/AuthController.cs` | `Api/Controllers/AuthController.cs` |
| `Models/*.cs` (DTOs) | `Shared/Contracts/DTOs/*.cs` |

## Next Steps

1. ✅ **Completed**: Basic structure and Login feature
2. 🔄 **In Progress**: Dashboard queries
3. ⏳ **TODO**: 
   - Add more features (Register, Password Reset)
   - Add Domain Events
   - Add Unit Tests
   - Add Integration Tests
   - Add Logging Middleware
   - Add Exception Handling Middleware

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

