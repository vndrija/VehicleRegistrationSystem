# eUprava Vehicle Registration System - Startup Guide

## Quick Start (Docker - Recommended)

### Prerequisites
- Docker & Docker Compose installed
- Port 5000, 5001, 5002, 5003, 1433 available

### Step 1: Start All Services
```bash
cd C:\Users\Lenovo\Desktop\projekti\eUprava
docker-compose up --build
```

**Wait for all services to start.** You'll see:
```
vehicleservice        | Listening on http://0.0.0.0:8080
authservice           | Listening on http://0.0.0.0:8080
notificationservice   | Listening on http://0.0.0.0:8080
trafficpoliceservice  | Listening on http://0.0.0.0:8080
sqlserver             | SQL Server is now ready for client connections
```

### Step 2: Start Frontend (in new terminal)
```bash
cd C:\Users\Lenovo\Desktop\projekti\eUprava\VehicleFrontend
npm install  # only first time
ng serve
```

Frontend will be at: **http://localhost:4200**

### Step 3: Register a Test User
1. Go to http://localhost:4200/register
2. Register with:
   ```
   Username: testuser
   Email: testuser@example.com
   Password: TestUser@123
   ```
3. Login and explore

---

## Service Endpoints

Once running, you can access:

| Service | URL | Purpose |
|---------|-----|---------|
| **Frontend** | http://localhost:4200 | Angular UI |
| **AuthService** | http://localhost:5000/swagger | User authentication |
| **VehicleService** | http://localhost:5001/swagger | Vehicle management |
| **NotificationService** | http://localhost:5002/swagger | Email notifications |
| **TrafficPoliceService** | http://localhost:5003/swagger | Police database |
| **SQL Server** | localhost:1433 | Database (see below) |
| **Mailtrap** | https://mailtrap.io | Email inbox (test emails) |

---

## Database Connection

### Connect to Docker SQL Server

**Credentials:**
- Server: `localhost,1433`
- Username: `sa`
- Password: `YourStrong@Passw0rd`
- Database: `VehicleDb`

**Important:** Add `TrustServerCertificate=True` to your connection string

**Using SQL Server Management Studio (SSMS):**
```
Server: localhost,1433
Authentication: SQL Server Authentication
Login: sa
Password: YourStrong@Passw0rd
```

**Using Azure Data Studio:**
- Connection type: Microsoft SQL Server
- Server: localhost,1433
- Authentication: SQL Server Authentication
- User name: sa
- Password: YourStrong@Passw0rd
- Trust server certificate: ✓ (checked)

---

## Testing Notification Service

The system automatically sends email notifications. Test it:

### 1. Create Test Data

Run these SQL queries in your database:

```sql
-- Create system_service user (for automated notifications)
INSERT INTO [dbo].[Users] ([Username], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES ('system_service', 'system@euprava.service',
  '$2a$11$NyuXZJQrZ.LZvxJJ8Z7Yme7pBL6QqMCCsKmPT5O5C5P5C5P5C5P5C',
  'Admin', 1, GETDATE());

-- Create test vehicle that expires in 15 days
INSERT INTO [dbo].[Vehicles]
([RegistrationNumber], [Make], [Model], [Year], [OwnerName], [ExpirationDate], [OwnerId], [Status])
VALUES
('TEST001', 'Toyota', 'Corolla', 2022, 'testuser',
  DATEADD(day, 15, GETDATE()), 1, 'Registered');
```

### 2. Wait for Background Job

The background service runs **every minute** and automatically:
1. Finds vehicles expiring within 30 days
2. Gets owner email from AuthService
3. Sends notification via NotificationService
4. Logs result in Docker output

### 3. Check Email Notification

Go to **https://mailtrap.io** and check your inbox for the email.

You should see in Docker logs:
```
Found 1 vehicles expiring within 30 days
Successfully sent notification to owner
Expiration notification batch complete: 1 sent, 0 failed
```

---

## Stopping Services

```bash
# Stop all services (keeps data)
docker-compose down

# Stop and remove volumes (deletes database data)
docker-compose down -v
```

---

## Local Development (Without Docker)

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- SQL Server 2022 (LocalDB or full)
- Visual Studio or VS Code

### Step 1: Update Connection Strings

Each service uses LocalDB by default. Update if needed in:
- `AuthService/appsettings.json`
- `VehicleService/appsettings.json`
- `NotificationService/appsettings.json`
- `TrafficPoliceService/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VehicleRegistrationDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 2: Apply Migrations

```bash
cd VehicleService
dotnet ef database update

cd ../AuthService
dotnet ef database update

cd ../NotificationService
dotnet ef database update

cd ../TrafficPoliceService
dotnet ef database update
```

### Step 3: Run Each Service

**Terminal 1 - AuthService:**
```bash
cd AuthService
dotnet run  # runs on https://localhost:5000
```

**Terminal 2 - VehicleService:**
```bash
cd VehicleService
dotnet run  # runs on https://localhost:5001
```

**Terminal 3 - NotificationService:**
```bash
cd NotificationService
dotnet run  # runs on https://localhost:5002
```

**Terminal 4 - TrafficPoliceService:**
```bash
cd TrafficPoliceService
dotnet run  # runs on https://localhost:5003
```

**Terminal 5 - Frontend:**
```bash
cd VehicleFrontend
ng serve  # runs on http://localhost:4200
```

---

## Troubleshooting

### Issue: "Cannot connect to database"
**Solution:** Make sure SQL Server is running and connection string is correct.

### Issue: "Port already in use (5000, 5001, etc)"
**Solution:**
```bash
# Find process using port (Windows)
netstat -ano | findstr :5000
# Kill it
taskkill /PID <PID> /F
```

### Issue: "Frontend shows 404 errors for API calls"
**Solution:** Check that backend services are running. Verify ports in `src/environments/environment.ts`

### Issue: "No emails in Mailtrap inbox"
**Solution:**
1. Check Docker logs: `docker-compose logs vehicleservice`
2. Look for authentication error: "Failed to authenticate service account"
3. If found, verify system_service user exists in database with correct password
4. Restart Docker: `docker-compose up --build`

### Issue: "Vehicles not found for notifications"
**Solution:** Make sure vehicles have:
- Status = `'Registered'` (not 'Unregistered')
- ExpirationDate within next 30 days
- OwnerId matches a real user

---

## System Features

### User Features
- ✅ Register & Login with JWT authentication
- ✅ View & manage vehicles
- ✅ Submit registration requests
- ✅ Transfer vehicle ownership
- ✅ Change license plates (with digital signature & PDF download)
- ✅ Automatic expiration notifications (email)
- ✅ View police violations & wanted status

### Admin Features
- ✅ Review registration requests (Approve/Reject)
- ✅ View all vehicles
- ✅ View notification history
- ✅ Access police database

### System Features
- ✅ Automatic daily expiration warnings
- ✅ Email notifications (Mailtrap)
- ✅ JWT token refresh mechanism
- ✅ Police integration with violations tracking
- ✅ Digital signatures for non-repudiation
- ✅ PDF document generation

---

## Important Files

```
eUprava/
├── docker-compose.yml          # Service configuration
├── VehicleService/
│   ├── Services/ExpirationNotificationBackgroundService.cs  # Background job
│   ├── Controllers/
│   └── Models/
├── AuthService/
├── NotificationService/
├── TrafficPoliceService/
├── VehicleFrontend/
│   ├── src/environments/        # API configuration
│   ├── src/app/
│   │   ├── services/            # HTTP clients
│   │   └── components/
│   └── Dockerfile
└── diagrams/                    # Architecture diagrams
```

---

## Next Steps

1. **Explore the UI** - Login and test features
2. **Test APIs** - Use Swagger UI at each service's `/swagger` endpoint
3. **Check Database** - Connect to SQL Server and explore tables
4. **Monitor Notifications** - Watch Docker logs for background job execution
5. **Review Code** - Check service implementations in `*/Controllers/` and `*/Services/`

---

## Support

- **Swagger UI**: Each service has API documentation at `/swagger`
- **Logs**: Check Docker output: `docker-compose logs -f <service-name>`
- **Database**: Connect via SSMS or Azure Data Studio
- **Emails**: View test emails in Mailtrap inbox

---

**Happy testing! 🚗**
