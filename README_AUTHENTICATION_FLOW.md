# User Authentication Flow Documentation

## Overview
This document describes the complete user authentication flow implemented in the application, including JWT-based authentication, dashboard access control, and service architecture.

## Architecture

### Service Layer (Clean Architecture)

```
Services/
├── IUserService.cs              # User management (CRUD operations)
├── UserService.cs               
├── IUserProfileService.cs       # User profile operations
├── UserProfileService.cs        
├── IUserDashboardService.cs     # Dashboard-specific operations (NEW)
├── UserDashboardService.cs      # Aggregates dashboard data (NEW)
├── IAuthService.cs              # Authentication logic
├── AuthService.cs               
├── IJwtService.cs               # JWT token operations
└── JwtService.cs                
```

### Controller Layer

```
Controllers/
├── AuthController.cs            # Authentication endpoints (login, logout, validate)
├── DashboardController.cs       # MVC Dashboard (requires [Authorize])
├── DashboardApiController.cs    # API Dashboard endpoints (NEW)
└── UserApiController.cs         # User profile API endpoints
```

## Authentication Flow

### 1. Login Process

#### Frontend (Views/Home/Index.cshtml)
1. User fills login form (email/username + password)
2. Form submission via JavaScript:
   ```javascript
   POST /api/auth/login
   Body: { EmailOrUsername, Password, RememberMe }
   ```

#### Backend (AuthController.cs)
1. Validates request data
2. Calls `AuthService.LoginAsync()`
3. `AuthService`:
   - Validates credentials via `UserService`
   - Generates JWT token via `JwtService`
   - Returns `AuthResponse` with token

#### Response Handling
1. Token stored in HTTP-only cookie (`AuthToken`)
2. Token also stored in localStorage (optional, for API calls)
3. User redirected to `/Dashboard`

### 2. Dashboard Access

#### Protection Mechanism
- `DashboardController` uses `[Authorize]` attribute
- JWT token read from cookie via `JwtBearerEvents.OnMessageReceived`
- If token invalid/missing → redirect to `/Home`

#### Data Loading
- `DashboardController.Index()` calls `UserDashboardService.GetDashboardDataAsync()`
- Service aggregates:
  - User profile
  - Account summary
  - Recent activities

### 3. Logout Process

#### Frontend
- User clicks logout button
- Calls `POST /api/auth/logout` or `POST /Dashboard/Logout`
- Clears token from localStorage
- Deletes cookies
- Redirects to home page

## API Endpoints

### Authentication Endpoints

#### POST /api/auth/login
**Request:**
```json
{
  "emailOrUsername": "admin",
  "password": "Admin123!",
  "rememberMe": false
}
```

**Response (Success):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com"
  },
  "expiresAt": "2024-01-01T01:00:00Z",
  "message": "ورود با موفقیت انجام شد"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
}
```

#### POST /api/auth/logout
- Clears authentication cookies
- Returns success message

#### GET /api/auth/validate
- Validates current JWT token
- Returns user information if valid

### Dashboard Endpoints (Protected)

#### GET /api/dashboard/data
- Returns complete dashboard data
- Requires: JWT authentication

#### GET /api/dashboard/profile
- Returns user profile
- Requires: JWT authentication

#### GET /api/dashboard/summary
- Returns account summary
- Requires: JWT authentication

#### GET /api/dashboard/activities
- Returns recent activities
- Requires: JWT authentication

## Service Responsibilities

### UserDashboardService
**Purpose:** Centralized service for dashboard operations

**Responsibilities:**
- Aggregates dashboard data from multiple sources
- Provides clean interface for dashboard controllers
- Easy to extend for future features (notifications, widgets, etc.)

**Methods:**
- `GetDashboardDataAsync()` - Complete dashboard view model
- `GetUserProfileAsync()` - User profile
- `GetAccountSummaryAsync()` - Account statistics
- `GetRecentActivitiesAsync()` - Activity log
- `UpdateUserProfileAsync()` - Profile updates

**Future Extensibility:**
```csharp
// Can easily add:
- GetNotificationsAsync()
- GetUserPreferencesAsync()
- GetUserRolesAsync()
- GetDashboardWidgetsAsync()
```

### AuthService
**Purpose:** Handles authentication logic

**Responsibilities:**
- Validates user credentials
- Manages login/logout operations
- Token validation

### UserService
**Purpose:** User data management

**Responsibilities:**
- User CRUD operations
- Password validation
- User lookup

## Security Features

### JWT Token Security
- ✅ HTTP-only cookies (prevents XSS)
- ✅ Secure flag (HTTPS only in production)
- ✅ SameSite=Strict (prevents CSRF)
- ✅ Token expiration (1 hour default, 7 days with RememberMe)
- ✅ Token validation on each request

### Password Security
- ✅ PBKDF2 hashing (10,000 iterations)
- ✅ HMAC-SHA256
- ✅ Salt per password
- ✅ No plain text storage

### Authorization
- ✅ `[Authorize]` attribute on protected endpoints
- ✅ JWT token validation
- ✅ Automatic redirect for unauthorized users

## Frontend Integration

### JavaScript Auth Service (wwwroot/js/auth.js)
Utility functions for token management:
- `AuthService.setToken(token)` - Store token
- `AuthService.getToken()` - Retrieve token
- `AuthService.clearToken()` - Remove token
- `AuthService.isAuthenticated()` - Check auth status
- `AuthService.logout()` - Logout and redirect

### Login Flow
1. User submits form
2. JavaScript sends POST to `/api/auth/login`
3. On success:
   - Token stored in cookie (automatic)
   - Token stored in localStorage (optional)
   - Redirect to `/Dashboard`

### Dashboard Access
1. User navigates to `/Dashboard`
2. `[Authorize]` attribute checks JWT token
3. If valid → load dashboard
4. If invalid → redirect to `/Home`

## Error Handling

### Invalid Credentials
- Returns 401 Unauthorized
- Message: "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
- Frontend displays error message

### Unauthorized Access
- Dashboard endpoints return 401
- MVC Dashboard redirects to home page
- Logs warning for security monitoring

### Token Expiration
- Token validation fails
- User redirected to login
- Can implement refresh token flow (future)

## Testing

### Manual Testing
1. **Login Test:**
   - Navigate to home page
   - Click "ورود" button
   - Enter: `admin` / `Admin123!`
   - Should redirect to dashboard

2. **Dashboard Access Test:**
   - Try accessing `/Dashboard` without login
   - Should redirect to home page

3. **Logout Test:**
   - Click logout in dashboard
   - Should clear session and redirect to home

### API Testing
```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUsername":"admin","password":"Admin123!","rememberMe":false}'

# Get Dashboard Data (requires token in cookie)
curl -X GET https://localhost:5001/api/dashboard/data \
  -H "Cookie: AuthToken=YOUR_TOKEN_HERE"
```

## Future Enhancements

### Planned Features
1. **Refresh Tokens**
   - Long-lived refresh tokens
   - Automatic token renewal

2. **Role-Based Access Control (RBAC)**
   - User roles (Admin, User, etc.)
   - Role-based dashboard customization

3. **Notifications**
   - Real-time notifications
   - Notification preferences

4. **User Preferences**
   - Dashboard layout customization
   - Theme preferences
   - Language settings

5. **Activity Logging**
   - Comprehensive audit trail
   - Security event logging

## Best Practices Implemented

✅ **Separation of Concerns**
- Services handle business logic
- Controllers handle HTTP concerns
- Models represent data

✅ **Dependency Injection**
- All services registered in DI container
- Easy to test and mock

✅ **Error Handling**
- Comprehensive try-catch blocks
- Meaningful error messages
- Proper HTTP status codes

✅ **Logging**
- Structured logging throughout
- Security event logging
- Error tracking

✅ **Security**
- Secure password hashing
- JWT token security
- Input validation
- XSS/CSRF protection

✅ **Extensibility**
- Interface-based design
- Service composition
- Easy to add new features

## Troubleshooting

### Login Fails
1. Check logs for validation errors
2. Verify user exists in system
3. Test password hashing: `/api/test/password`
4. Test user lookup: `/api/test/user`

### Dashboard Access Denied
1. Verify JWT token in cookies
2. Check token expiration
3. Verify `[Authorize]` attribute
4. Check JWT configuration in `Program.cs`

### Token Not Stored
1. Check cookie settings (HttpOnly, Secure, SameSite)
2. Verify HTTPS in production
3. Check browser console for errors

