# Docker Setup Guide - eUprava Vehicle Registration System

## What Was Fixed

All Docker configuration issues have been resolved. Your system is now ready to run in Docker!

### ✅ Fixes Applied

#### 1. docker-compose.yml - Complete Configuration
- ✅ **AuthService**: Added JWT configuration (SecretKey, Issuer, Audience, ExpireMinutes, RefreshTokenExpireDays)
- ✅ **VehicleService**: Added inter-service URLs (AuthService, NotificationService, **TrafficPoliceService**) and JWT configuration
- ✅ **NotificationService**: Added database connection string + Email/SMTP configuration (Mailtrap)
- ✅ **TrafficPoliceService**: Added (NEW) - Police database integration service
- ✅ **Dependencies**: Fixed service startup order (vehicleservice depends on authservice, notificationservice, trafficpoliceservice)

#### 2. Automatic Database Migrations
All three services now automatically create/update databases on startup:
- ✅ **AuthService**: Already had automatic migrations (Program.cs:64-68)
- ✅ **VehicleService**: Uncommented automatic migrations (Program.cs:83-87)
- ✅ **NotificationService**: Added automatic migrations (Program.cs:37-42)

#### 3. Inter-Service Communication
- ✅ VehicleService can now communicate with AuthService at `http://authservice:8080`
- ✅ VehicleService can now communicate with NotificationService at `http://notificationservice:8080`
- ✅ Configuration uses Docker container names (not localhost)

---

## Docker Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                    Docker Network: euprava-network             │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │ AuthService  │  │VehicleService│  │Notification  │        │
│  │  Port: 5000  │  │  Port: 5001  │  │Service       │        │
│  │  (→8080)     │  │  (→8080)     │  │Port: 5002    │        │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘        │
│         │                 │                  │                 │
│         │                 │    ┌─────────────┼─────────────┐  │
│         │                 │    │             │             │  │
│         │                 │    │      ┌──────┴─────┐      │  │
│         │                 │    │      │            │      │  │
│         └─────────────────┴────┼──────┤  Traffic   │      │  │
│                           │    │      │  Police   │      │  │
│                           │    │      │  Service  │      │  │
│                           │    │      │  Port:   │      │  │
│                  ┌────────▼────┴──────┤  5003    │      │  │
│                  │  SQL Server │      │(→8080)   │      │  │
│                  │  Port: 1433 │      │          │      │  │
│                  │  Container  │      └──────────┘      │  │
│                  │  Volume:    │                        │  │
│                  │  sqlserver- │                        │  │
│                  │  data       │                        │  │
│                  └─────────────┘                        │  │
│                                 ┌──────────────────────┘  │
│  ┌────────────────────────────────────────────────┐       │
│  │         VehicleFrontend (Angular)              │       │
│  │         Port: 4200 (→80)                       │       │
│  └────────────────────────────────────────────────┘       │
└────────────────────────────────────────────────────────────┘
```

---

## How to Run Docker

### Step 1: Ensure Docker Desktop is Running
Make sure Docker Desktop is started on your machine.

### Step 2: Build and Start All Services
```bash
# Navigate to project root
cd C:\Users\Lenovo\Desktop\projekti\eUprava

# Build and start all containers (first time)
docker-compose up --build

# OR run in detached mode (background)
docker-compose up -d --build
```

### Step 3: Wait for Services to Start
Monitor the logs to see when services are ready:
```bash
docker-compose logs -f
```

Look for these messages:
- ✅ `authservice     | Now listening on: http://[::]:8080`
- ✅ `vehicleservice  | Now listening on: http://[::]:8080`
- ✅ `notificationservice | Now listening on: http://[::]:8080`
- ✅ `trafficpoliceservice | Now listening on: http://[::]:8080`
- ✅ Database migrations applied automatically

### Step 4: Access the Services

**Frontend (Angular):**
- URL: http://localhost:4200

**Backend APIs (Swagger):**
- AuthService: http://localhost:5000/swagger
- VehicleService: http://localhost:5001/swagger
- NotificationService: http://localhost:5002/swagger
- TrafficPoliceService: http://localhost:5003/swagger

**SQL Server:**
- Server: `localhost,1433`
- Username: `sa`
- Password: `YourStrong@Passw0rd`
- Connect via SQL Server Management Studio (SSMS)

---

## Docker Commands Reference

### Starting Services
```bash
# Start all services
docker-compose up

# Start in background (detached mode)
docker-compose up -d

# Rebuild images and start
docker-compose up --build

# Start specific service only
docker-compose up authservice
```

### Stopping Services
```bash
# Stop all services (containers remain)
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop and remove containers + volumes (DELETES DATABASE!)
docker-compose down -v
```

### Viewing Logs
```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View logs for specific service
docker-compose logs -f authservice
docker-compose logs -f vehicleservice
docker-compose logs -f notificationservice
docker-compose logs -f trafficpoliceservice
```

### Container Management
```bash
# List running containers
docker-compose ps

# Execute command in running container
docker exec -it authservice bash

# View container resource usage
docker stats
```

### Database Management
```bash
# Connect to SQL Server container
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"

# Backup database volume
docker run --rm -v sqlserver-data:/data -v $(pwd):/backup ubuntu tar cvf /backup/backup.tar /data

# Restore database volume
docker run --rm -v sqlserver-data:/data -v $(pwd):/backup ubuntu tar xvf /backup/backup.tar -C /
```

---

## What Happens on Startup (Behind the Scenes)

### 1. SQL Server Container Starts First
- Creates empty SQL Server instance
- Mounts persistent volume `sqlserver-data`
- Waits for connections on port 1433

### 2. AuthService Starts
- Connects to SQL Server: `Server=sqlserver;Database=AuthDb;...`
- **Automatically creates AuthDb database** (via migrations)
- Creates `Users` table
- Creates `__EFMigrationsHistory` tracking table
- Starts API on port 8080 (mapped to 5000 externally)

### 3. NotificationService Starts
- Connects to SQL Server: `Server=sqlserver;Database=NotificationDb;...`
- **Automatically creates NotificationDb database** (via migrations)
- Creates `Notifications` table
- Loads Email/SMTP configuration from environment variables
- Starts API on port 8080 (mapped to 5002 externally)

### 4. TrafficPoliceService Starts
- **No database required** (uses hardcoded in-memory data)
- Contains wanted vehicles and traffic violations data
- Provides police verification endpoints for VehicleService
- Starts API on port 8080 (mapped to 5003 externally)

### 5. VehicleService Starts (waits for AuthService + NotificationService + TrafficPoliceService)
- Connects to SQL Server: `Server=sqlserver;Database=VehicleDb;...`
- **Automatically creates VehicleDb database** (via migrations)
- Creates tables: `Vehicles`, `RegistrationRequests`, `VehicleTransfers`, `VehicleOwnershipHistory`
- Configures HttpClient to communicate with:
  - AuthService at `http://authservice:8080`
  - NotificationService at `http://notificationservice:8080`
  - TrafficPoliceService at `http://trafficpoliceservice:8080`
- Loads JWT configuration for authentication
- Starts API on port 8080 (mapped to 5001 externally)

### 6. VehicleFrontend Starts (Angular)
- Builds Angular application
- Serves on port 80 (mapped to 4200 externally)
- Connects to backend APIs via localhost:5000, localhost:5001, localhost:5002

---

## Database Structure in Docker

| Database | Tables | Purpose |
|----------|--------|---------|
| **AuthDb** | Users, __EFMigrationsHistory | User authentication and management |
| **VehicleDb** | Vehicles, RegistrationRequests, VehicleTransfers, VehicleOwnershipHistory, __EFMigrationsHistory | Vehicle registration system |
| **NotificationDb** | Notifications, __EFMigrationsHistory | Email notification tracking |

**Note:** `__EFMigrationsHistory` table tracks which migrations have been applied.

---

## Environment Variables (Set in docker-compose.yml)

### AuthService
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Jwt__SecretKey`: Secret key for JWT signing
- `Jwt__Issuer`: JWT token issuer
- `Jwt__Audience`: JWT token audience
- `Jwt__ExpireMinutes`: Access token expiration (60 minutes)
- `Jwt__RefreshTokenExpireDays`: Refresh token expiration (7 days)

### VehicleService
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Services__AuthService`: URL to AuthService (`http://authservice:8080`)
- `Services__NotificationService`: URL to NotificationService (`http://notificationservice:8080`)
- `Services__TrafficPoliceService`: URL to TrafficPoliceService (`http://trafficpoliceservice:8080`)
- `Jwt__SecretKey`: Same secret key as AuthService for token validation
- `Jwt__Issuer`: JWT token issuer
- `Jwt__Audience`: JWT token audience

### NotificationService
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Email__SenderName`: Email sender display name
- `Email__SenderEmail`: Email sender address
- `Email__SmtpHost`: SMTP server (sandbox.smtp.mailtrap.io)
- `Email__SmtpPort`: SMTP port (587)
- `Email__Username`: SMTP username (Mailtrap credentials)
- `Email__Password`: SMTP password (Mailtrap credentials)

---

## Troubleshooting

### Problem: "Cannot connect to SQL Server"
**Solution:** Wait 10-20 seconds for SQL Server to fully start, then restart the service:
```bash
docker-compose restart authservice
```

### Problem: "Database does not exist"
**Solution:** Migrations should run automatically. Check logs:
```bash
docker-compose logs authservice | grep -i migration
```
If no migrations ran, restart the service.

### Problem: "Port already in use"
**Solution:** Stop the local service (dotnet run) before starting Docker:
```bash
# Kill processes on ports 5000, 5001, 5002
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Problem: "Container keeps restarting"
**Solution:** Check logs for errors:
```bash
docker-compose logs <service-name>
```
Common issues:
- Missing environment variable
- Database connection failed
- Application crash on startup

### Problem: "VehicleService can't reach AuthService"
**Solution:** Ensure all services are on the same network:
```bash
docker network inspect euprava-network
```
All containers should be listed. If not, restart:
```bash
docker-compose down
docker-compose up
```

### Problem: "Emails not sending"
**Solution:** Check NotificationService logs for SMTP errors:
```bash
docker-compose logs notificationservice | grep -i email
```
Verify Mailtrap credentials are correct in docker-compose.yml.

---

## Testing the Docker Setup

### 1. Register a User
```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test123!",
  "role": "User"
}
```

### 2. Login
```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "Test123!"
}
```
Copy the `token` from the response.

### 3. Create a Vehicle
```bash
POST http://localhost:5001/api/vehicles
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "registrationNumber": "BG-1234-AB",
  "make": "Toyota",
  "model": "Corolla",
  "year": 2020
}
```

### 4. Check Database in SSMS
1. Open SQL Server Management Studio
2. Connect to: `localhost,1433`
3. Login: `sa` / `YourStrong@Passw0rd`
4. Expand Databases → See AuthDb, VehicleDb, NotificationDb
5. Query data:
```sql
-- Check users
USE AuthDb;
SELECT * FROM Users;

-- Check vehicles
USE VehicleDb;
SELECT * FROM Vehicles;

-- Check notifications
USE NotificationDb;
SELECT * FROM Notifications;
```

---

## Differences: Local vs Docker

| Aspect | Local (dotnet run) | Docker (docker-compose) |
|--------|-------------------|------------------------|
| Database Engine | LocalDB | SQL Server 2022 Container |
| Server Name | `(localdb)\MSSQLLocalDB` | `localhost,1433` |
| Auth | Windows Authentication | SQL Authentication (sa) |
| Vehicle DB Name | `VehicleRegistrationDb` | `VehicleDb` ⚠️ Different! |
| Inter-service URLs | `localhost:5278`, `localhost:5002` | `authservice:8080`, `notificationservice:8080` |
| Configuration | appsettings.json | Environment variables in docker-compose.yml |
| Data Persistence | User AppData folder | Docker volume `sqlserver-data` |
| Migrations | Manual (`dotnet ef database update`) | Automatic on container startup |

---

## Next Steps

1. ✅ **Docker is ready** - All fixes applied
2. 🚀 **Test locally first** - Make sure `dotnet run` works for each service
3. 🐳 **Run Docker** - `docker-compose up --build`
4. 📊 **Verify databases** - Connect to SQL Server in SSMS
5. 🧪 **Test API endpoints** - Use Swagger UI or Postman
6. 📧 **Check Mailtrap** - Verify emails are being captured

---

## Summary of All Fixes

✅ **docker-compose.yml** (6 fixes):
1. NotificationService: Added database connection string
2. NotificationService: Added Email/SMTP configuration
3. VehicleService: Added inter-service URLs (AuthService, NotificationService)
4. VehicleService: Added JWT configuration
5. AuthService: Added JWT configuration
6. VehicleService: Added service dependencies (authservice, notificationservice)

✅ **Program.cs files** (2 fixes):
1. VehicleService/Program.cs: Uncommented automatic migration code
2. NotificationService/Program.cs: Added automatic migration code

✅ **Configuration mapping**:
- Environment variables use `__` (double underscore) to map to nested config keys
- Example: `Services__AuthService` → `builder.Configuration["Services:AuthService"]`

---

**Your Docker environment is now fully configured and ready to use!** 🎉
