# Quick Setup Guide

## Backend Setup (5 minutes)

### Step 1: Install Entity Framework Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

### Step 2: Create Database Migration
```bash
dotnet ef migrations add InitialCreate --project . --startup-project .
```

### Step 3: Update Database
```bash
dotnet ef database update
```

### Step 4: Run the Application
```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`

## Testing the Application

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Test the `/api/auth/register` endpoint to create a user
3. Test the `/api/auth/login` endpoint to get a JWT token
4. Test the `/api/auth/forgot-password` endpoint
5. Check console logs (or email if configured) for reset token
6. Test the `/api/auth/reset-password` endpoint with the token

### Using Postman/HTTP Client

1. **Register a User**
   ```
   POST http://localhost:5000/api/auth/register
   Content-Type: application/json
   
   {
     "email": "test@example.com",
     "password": "SecurePass123",
     "firstName": "John",
     "lastName": "Doe",
     "role": "Customer"
   }
   ```

2. **Login**
   ```
   POST http://localhost:5000/api/auth/login
   Content-Type: application/json
   
   {
     "email": "test@example.com",
     "password": "SecurePass123"
   }
   ```
   
   Copy the `token` from the response.

3. **Use Token for Protected Endpoints**
   ```
   Authorization: Bearer <your-jwt-token>
   ```

## Important Configuration

### Backend (`appsettings.json`)

1. **JWT Secret Key**: Change the default secret key to a secure random string (at least 32 characters)
2. **Database Connection**: Update connection string if not using LocalDB
3. **Email Settings**: Configure SMTP for production (optional for development)
4. **CORS**: Update CORS origins in `Program.cs` if your frontend runs on a different port

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server LocalDB is installed
- Check connection string in `appsettings.json`
- Try creating the database manually if migrations fail

### CORS Issues
- Update CORS configuration in `Program.cs` to match your frontend URL
- Check browser console for CORS errors

### Email Not Sending
- In development, check console logs for reset tokens
- Configure SMTP settings in `appsettings.json` for production
- For Gmail, use an App Password (not your regular password)

### JWT Token Issues
- Ensure JWT secret key is at least 32 characters
- Check token expiration settings
- Verify token is being sent in Authorization header

## Next Steps

After successful setup:
1. Test all authentication flows using Swagger or Postman
2. Configure email service for production
3. Update security settings (JWT secret, CORS, etc.)
4. Integrate with your frontend application
5. Add additional features as needed
