# E-Commerce Authentication & Authorization Module

A complete authentication and authorization system built with **ASP.NET Core 8**, following Clean Architecture principles.

## Features

✅ **User Registration** - Secure user registration with email validation  
✅ **User Login** - JWT-based authentication  
✅ **Forgot Password** - Secure password reset token generation  
✅ **Reset Password** - Time-limited, single-use password reset  
✅ **Role-Based Authorization** - Admin, Seller, and Customer roles  
✅ **Secure Password Hashing** - PBKDF2 with salt  
✅ **JWT Token Authentication** - Secure token-based access  
✅ **Email Notifications** - Password reset emails  

## Project Structure

```
E-Commerce/
├── Core/                   # Business logic layer
│   ├── Entities/          # Domain entities
│   ├── DTOs/              # Data transfer objects
│   ├── Interfaces/        # Service contracts
│   ├── Services/          # Business services
│   ├── Helpers/           # Utility classes
│   └── Exceptions/        # Custom exceptions
├── Infrastructure/        # Data & external services
│   ├── Data/              # DbContext
│   ├── Repositories/      # Data access
│   └── Services/          # Email, JWT services
├── Controllers/           # API endpoints
└── Middleware/           # Exception handling
```

## Backend Setup

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json` with your database connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceDb;Trusted_Connection=True;"
     }
   }
   ```

3. **Configure JWT Settings**
   
   Update `appsettings.json` with a secure secret key (at least 32 characters):
   ```json
   {
     "JwtSettings": {
       "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLongForSecurity",
       "Issuer": "E-Commerce-API",
       "Audience": "E-Commerce-Client",
       "ExpiryMinutes": "1440"
     }
   }
   ```

4. **Configure Email Settings (Optional)**
   
   For password reset emails, configure SMTP settings:
   ```json
   {
     "EmailSettings": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": "587",
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password",
       "FromEmail": "your-email@gmail.com",
       "FromName": "E-Commerce App"
     }
   }
   ```
   
   **Note:** If email is not configured, reset tokens will be logged to the console in development mode.

5. **Create Database Migration**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Run the Application**
   ```bash
   dotnet run
   ```
   
   The API will be available at `https://localhost:5001` or `http://localhost:5000`

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/forgot-password` | Request password reset |
| POST | `/api/auth/reset-password` | Reset password with token |

### Request/Response Examples

#### Register
```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Customer"
}
```

#### Login
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "SecurePass123"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Customer",
    "expiresAt": "2024-01-02T12:00:00Z"
  }
}
```

#### Forgot Password
```json
POST /api/auth/forgot-password
{
  "email": "user@example.com"
}
```

#### Reset Password
```json
POST /api/auth/reset-password
{
  "token": "reset-token-from-email",
  "email": "user@example.com",
  "newPassword": "NewSecurePass123"
}
```

## Security Features

- ✅ **Password Hashing**: PBKDF2 with 100,000 iterations and SHA-256
- ✅ **JWT Tokens**: Secure token-based authentication
- ✅ **Token Expiry**: Configurable token expiration (default 24 hours)
- ✅ **Reset Token Security**: Single-use, time-limited (1 hour) reset tokens
- ✅ **Email Enumeration Protection**: Forgot password doesn't reveal if email exists
- ✅ **Active User Check**: Only active users can log in
- ✅ **Unique Email**: Email addresses must be unique per user

## User Roles

- **Admin**: Full system access
- **Seller**: Seller-specific features
- **Customer**: Standard user access

## Testing

### Using Swagger

1. Run the backend application
2. Navigate to `https://localhost:5001/swagger`
3. Use the Swagger UI to test API endpoints

### Using Postman/HTTP Client

Import the API endpoints and test with sample requests. Remember to include the JWT token in the Authorization header for protected endpoints:

```
Authorization: Bearer <your-jwt-token>
```

## Development Notes

- The backend uses **Clean Architecture** with clear separation of concerns
- JWT tokens are returned in the response (frontend should handle storage)
- Email service logs tokens to console if SMTP is not configured (development only)
- CORS is configured to allow requests from `http://localhost:4200` (update for your frontend)

## Production Considerations

1. **Change JWT Secret Key**: Use a strong, randomly generated secret key
2. **Configure HTTPS**: Always use HTTPS in production
3. **Email Service**: Configure proper SMTP settings for production
4. **Database**: Use a production-grade database (not LocalDB)
5. **CORS**: Update CORS policy to allow only your production frontend domain
6. **Rate Limiting**: Implement rate limiting for login/register endpoints
7. **Logging**: Configure proper logging and monitoring

## License

This project is for educational purposes.
