## E-Commerce API (ASP.NET Core + EF Core)

Backend for a small e‑commerce system with authentication, user management, products/categories, cart, orders, returns, and refund tracking.

### Tech stack
- **Runtime**: .NET 8 (`net8.0`)
- **Web**: ASP.NET Core Web API + Swagger
- **Data**: Entity Framework Core 8 + SQL Server
- **Auth**: JWT bearer tokens + Google ID token login

### Project structure
- **`Core`**: entities, DTOs, interfaces, services, helpers, exceptions
- **`Infrastructure`**: `ApplicationDbContext`, repositories, email/JWT helpers
- **`Controllers`**: HTTP endpoints
- **`Middleware`**: exception handling

### Prerequisites
- .NET 8 SDK
- SQL Server (Express / LocalDB / full)

### Configuration (`appsettings.json`)
- **Connection string**: `ConnectionStrings:DefaultConnection`
- **JWT**: `JwtSettings:SecretKey`, `Issuer`, `Audience`, `ExpiryMinutes`
- **Google login**: `GoogleOAuth:ClientId`
- **Email (optional)**: `EmailSettings:*` (if missing, OTP is logged instead of sent)

> **Important**: Replace any hard‑coded secrets with secure values (user secrets, environment variables) before real deployment.

### Database
```bash
dotnet tool restore           # if you use dotnet-ef as local tool
dotnet ef database update     # apply existing migrations
```

### Run the API
```bash
dotnet run
```
Swagger (in Development): `https://localhost:<port>/swagger`

---

### Feature overview (by module)

- **Auth (`AuthController`)**
  - Register, email/password login, **Google login**, forgot/reset password (OTP via email), JWT issuance.
  - Only **active** users can log in; passwords are hashed; roles: `Admin`, `Seller`, `Customer`.

- **Users (`UserController`)**
  - Get all users, get by id, activate/deactivate user.
  - Deactivated users are blocked at login.

- **Product categories (`ProductCategoryController`)**
  - Public: list categories, get by id.
  - Admin: create, update, delete categories.

- **Products (`ProductController`)**
  - Public / customer:
    - Browse approved products only, get product by id.
  - Seller / admin:
    - Create / update / delete products.
    - Seller can only manage own products; admin can manage all.
    - Admin approves/rejects products; only **approved** products are visible to customers.
  - Each product has `returnPolicyDays` (0 = no returns, >0 = days after delivery when returns are allowed).

- **Cart (`CartController`)**
  - Authenticated customer:
    - Get own cart, add item, update quantity, remove item, clear cart.

- **Orders & returns (`OrderController`)**
  - Customer:
    - Create order from current cart.
    - View own orders and order details.
    - Request return for an order item (only if order is delivered and within that product’s `returnPolicyDays`).
  - Admin:
    - View all orders.
    - Update order status (`Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`).
      - When set to **Delivered**, `DeliveredAt` is stored.
      - When set to **Cancelled**, stock is restored.
  - Seller/Admin:
    - Approve/reject return requests for order items.
    - On approval: item `ReturnStatus = Approved`, stock is increased.

- **Refund status (per order item)**
  - Fields on each order item (exposed in order DTOs):
    - **`ReturnStatus`**: `None | Requested | Approved | Rejected`
    - **`RefundStatus`**: `None | Initiated | Done | Refunded`
  - Flow:
    1. Customer requests a return.
    2. Seller/Admin approves it ⇒ backend sets `RefundStatus = "Initiated"`.
    3. Admin can update `RefundStatus` (including **"Refunded"**) so the customer clearly sees refund progress/state.

---

### API surface (high‑level)

- **Auth**
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/google-login`
  - `POST /api/auth/forgot-password`
  - `POST /api/auth/reset-password`

- **Users**
  - `GET /api/user`
  - `GET /api/user/{id}`
  - `PUT /api/user/status`

- **Categories**
  - `GET /api/productcategory`
  - `GET /api/productcategory/{id}`
  - `POST /api/productcategory`
  - `PUT /api/productcategory/{id}`
  - `DELETE /api/productcategory/{id}`

- **Products**
  - `GET /api/product` (role‑aware)
  - `GET /api/product/approved`
  - `GET /api/product/pending`
  - `GET /api/product/seller/{sellerId}`
  - `GET /api/product/{id}`
  - `POST /api/product`
  - `PUT /api/product/{id}`
  - `DELETE /api/product/{id}`
  - `POST /api/product/approve`

- **Cart**
  - `GET /api/cart`
  - `POST /api/cart`
  - `PUT /api/cart/{id}`
  - `DELETE /api/cart/{id}`
  - `DELETE /api/cart/clear`

- **Orders & returns**
  - `GET /api/order` (current user)
  - `GET /api/order/admin` (admin)
  - `GET /api/order/{id}`
  - `POST /api/order` (create from cart)
  - `PUT /api/order/status` (admin)
  - `POST /api/order/return` (request return)
  - `POST /api/order/return/resolve` (admin/seller)
  - `PUT /api/order/return/refund-status` (admin)

Use Swagger to see full request/response schemas for every endpoint.

# E-Commerce API (ASP.NET Core + EF Core)

Backend API for an E-Commerce system (Auth, Users, Products/Categories, Cart, Orders, Returns + Refund status).

## Tech stack

- **.NET**: `net8.0`
- **Database**: SQL Server (EF Core 8)
- **Auth**: JWT + Google ID token login
- **Docs**: Swagger (`/swagger` in Development)

## Prerequisites

- .NET SDK 8+
- SQL Server (LocalDB / SQLExpress / full SQL Server)

## Configure

Update `appsettings.json` (or `appsettings.Development.json`):

- **Connection string**
  - `ConnectionStrings:DefaultConnection`
- **JWT**
  - `JwtSettings:SecretKey` (use a strong value, 32+ chars)
  - `JwtSettings:Issuer`, `JwtSettings:Audience`, `JwtSettings:ExpiryMinutes`
- **Google login**
  - `GoogleOAuth:ClientId`
- **Email (optional)**
  - `EmailSettings:*`
  - If SMTP user/pass are missing, the app will log OTP instead of sending email.

> Security note: don’t commit real secrets in `appsettings.json`. Use User Secrets or environment variables in real deployments.

## Database setup (EF Core)

From the project folder:

```bash
dotnet tool restore
dotnet ef database update
```

## Run

```bash
dotnet run
```

Swagger UI (Development): `https://localhost:<port>/swagger`

## Roles & permissions (high level)

- **Customer**
  - View own orders, manage cart, request return within return window
- **Seller**
  - Manage own products (approval required for listing), resolve returns for own products
- **Admin**
  - Manage categories, approve products, manage all orders, update order status, update refund status, manage users

## Returns & refund status

Per order item fields (exposed on order APIs):

- **ReturnStatus**: `None | Requested | Approved | Rejected`
- **RefundStatus**: `None | Initiated | Done | Refunded`

Flow:

1. Customer requests return for an order item (must be delivered and within product return policy window).
2. Seller/Admin resolves (approve/reject).
3. When approved, backend sets `RefundStatus = Initiated`.
4. Admin can update `RefundStatus` (including **Refunded** so customer can clearly see “return refunded”).

## Common commands

```bash
dotnet build -c Release
dotnet ef migrations add <Name>
dotnet ef database update
```

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
