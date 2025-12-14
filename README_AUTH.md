# Authentication Service Documentation

## Overview
This project includes a complete, production-ready authentication service built with ASP.NET Core 7.0, featuring JWT-based authentication, secure password hashing, and a modular architecture.

## Features

### ✅ Implemented Features
- **User Authentication**: Email/Username and password login
- **Password Security**: ASP.NET Identity PasswordHasher with secure hashing
- **JWT Tokens**: Token-based authentication with configurable expiration
- **Remember Me**: Extended token expiration (7 days vs 1 hour)
- **Error Handling**: Comprehensive validation and error messages
- **API Endpoints**: RESTful API for frontend integration
- **Cookie Support**: Automatic token storage in secure HTTP-only cookies
- **Modular Design**: Clean separation of concerns with service interfaces

### 🔄 Extensible Features (Ready for Future Implementation)
- User Registration
- Password Reset/Forgot Password
- Email Verification
- Role-Based Access Control (RBAC)
- Two-Factor Authentication (2FA)
- Refresh Token Rotation
- Account Lockout
- Session Management

## Architecture

### Service Layer
```
Services/
├── IUserService.cs          # User management interface
├── UserService.cs           # User operations (CRUD, validation)
├── IJwtService.cs           # JWT token interface
├── JwtService.cs            # Token generation and validation
├── IAuthService.cs          # Authentication interface
└── AuthService.cs           # Main authentication logic
```

### Models
```
Models/
├── User.cs                  # User entity and DTO
├── AuthResponse.cs          # Authentication response model
└── LoginViewModel.cs        # Login form model
```

### Controllers
```
Controllers/
├── HomeController.cs        # MVC login endpoint
└── AuthController.cs        # API login endpoints
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "YOUR_SECRET_KEY_HERE_MIN_64_CHARACTERS",
    "Issuer": "GoldPriceTracker",
    "Audience": "GoldPriceTrackerUsers"
  }
}
```

**⚠️ Important**: Generate a strong secret key for production:
```csharp
var randomBytes = new byte[64];
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(randomBytes);
var secretKey = Convert.ToBase64String(randomBytes);
```

## Usage

### Demo User
A demo user is automatically created on first run:
- **Email/Username**: `admin@example.com` or `admin`
- **Password**: `Admin123!`

### API Endpoints

#### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "admin@example.com",
  "password": "Admin123!",
  "rememberMe": false
}
```

**Response (Success)**:
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64_refresh_token",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "expiresAt": "2024-01-01T01:00:00Z",
  "message": "ورود با موفقیت انجام شد"
}
```

**Response (Error)**:
```json
{
  "success": false,
  "message": "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
}
```

#### 2. Validate Token
```http
GET /api/auth/validate
Cookie: AuthToken=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### 3. Logout
```http
POST /api/auth/logout
```

### MVC Endpoint

#### Login (Form Submission)
```http
POST /Home/Login
Content-Type: application/x-www-form-urlencoded

EmailOrUsername=admin@example.com&Password=Admin123!&RememberMe=false
```

## Security Features

### Password Hashing
- Uses ASP.NET Identity `PasswordHasher<User>`
- PBKDF2 with HMAC-SHA256
- Automatic salt generation
- Iteration count: 10,000+ (default)

### JWT Security
- HMAC-SHA256 signing algorithm
- Configurable expiration times
- Token validation on each request
- Secure HTTP-only cookies
- SameSite=Strict cookie policy

### Best Practices
- ✅ No password storage in plain text
- ✅ Secure token storage (HTTP-only cookies)
- ✅ Input validation
- ✅ Error messages don't reveal user existence
- ✅ Logging for security events
- ✅ Token expiration
- ✅ HTTPS enforcement in production

## Extending the Service

### Adding User Registration

1. **Add to IUserService**:
```csharp
Task<User> CreateUserAsync(string username, string email, string password);
```

2. **Implement in UserService**:
```csharp
public async Task<User> CreateUserAsync(string username, string email, string password)
{
    // Check if user exists
    var existingUser = await GetUserByEmailOrUsernameAsync(email);
    if (existingUser != null)
        throw new InvalidOperationException("User already exists");
    
    // Create new user (already implemented)
    return await CreateUserAsync(username, email, password);
}
```

3. **Add to AuthService**:
```csharp
public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
{
    // Validation
    // Create user
    // Auto-login or send verification email
}
```

### Adding Password Reset

1. **Add ResetToken to User model**
2. **Create PasswordResetService**
3. **Add endpoints**: `/api/auth/forgot-password`, `/api/auth/reset-password`

### Adding Role-Based Access

1. **Add Role property to User model**
2. **Add roles to JWT claims**
3. **Use `[Authorize(Roles = "Admin")]` attributes**

## Database Integration

Currently uses in-memory storage. To integrate with a database:

1. **Install Entity Framework Core**:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

2. **Create DbContext**:
```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
}
```

3. **Update UserService** to use DbContext instead of static list

## Testing

### Manual Testing
1. Start the application
2. Navigate to the dashboard
3. Click "ورود" button
4. Use demo credentials: `admin@example.com` / `Admin123!`
5. Check browser cookies for `AuthToken`

### Unit Testing Example
```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var request = new LoginRequest 
    { 
        EmailOrUsername = "admin@example.com",
        Password = "Admin123!"
    };
    
    // Act
    var result = await _authService.LoginAsync(request);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Token);
}
```

## Troubleshooting

### Token Validation Fails
- Check JWT secret key matches in `appsettings.json`
- Verify token hasn't expired
- Ensure clock synchronization

### Password Validation Fails
- Verify password hashing algorithm matches
- Check user exists and is active
- Review logs for detailed error messages

### Cookie Not Set
- Ensure HTTPS in production
- Check SameSite cookie policy
- Verify cookie expiration time

## License
This authentication service is part of the GoldPriceTracker project.

