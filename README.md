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

### All APIs (by area)

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

