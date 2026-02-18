# Traffic Police Service Integration

## Overview

I've created a new **Traffic Police Service** that represents collaboration with law enforcement. This demonstrates real-world inter-agency communication where the vehicle registration system communicates with the traffic police to verify vehicle status and report vehicles.

## What Was Added

### 1. New Service: TrafficPoliceService (Port 5003)
Located at: `TrafficPoliceService/`

**Components**:
- `Models/` - WantedVehicle, TrafficViolation, PoliceVehicleReport
- `Controllers/PoliceVehicleController.cs` - 6 endpoints
- Hardcoded in-memory database with sample data

**Hardcoded Test Data**:
- **Wanted Vehicles**: 2 vehicles (BMW X5, Mercedes E-Class)
- **Traffic Violations**: 3 violations for sample vehicles
- Each endpoint returns realistic police database records

### 2. New Endpoints in TrafficPoliceService

#### Check Vehicle Status
```
GET /api/policevehicle/check-vehicle/{registrationNumber}
```
Returns: Is vehicle wanted? Outstanding fines? Violations?

#### Get Wanted Vehicles
```
GET /api/policevehicle/wanted-vehicles
```
Returns: List of all active wanted vehicles

#### Get Vehicle Violations
```
GET /api/policevehicle/violations/{registrationNumber}
```
Returns: All traffic violations for a vehicle

#### Report Vehicle to Police
```
POST /api/policevehicle/report-vehicle
```
VehicleService sends vehicle info to police when needed

#### Add Wanted Vehicle
```
POST /api/policevehicle/wanted-vehicles
```
Police officer adds vehicle to wanted list

#### Record Violation
```
POST /api/policevehicle/violations
```
Police officer records traffic violation

### 3. New Endpoints in VehicleService

#### Check Vehicle with Police
```
GET /api/vehicles/{id}/police-check
```
**Usage**: Verify vehicle status with police before approving registration
**Response**: Combined vehicle info + police report

#### Report Vehicle to Police
```
POST /api/vehicles/{id}/report-to-police
```
**Usage**: Send vehicle data to police (when transferring, registering, etc.)
**Response**: Confirmation that police received the report

## Architecture Diagram

```
┌─────────────────┐
│  VehicleService │
│   (Port 5001)   │
└────────┬────────┘
         │ HTTP calls
         │ /api/policevehicle/*
         ↓
┌─────────────────────────┐
│ TrafficPoliceService    │
│   (Port 5003)           │
├─────────────────────────┤
│ • Wanted Vehicles       │
│ • Traffic Violations    │
│ • Vehicle Status Check  │
└─────────────────────────┘
```

## How It Works

### Scenario 1: Checking Vehicle Before Registration
1. User requests vehicle registration in VehicleService
2. Admin can call: `GET /api/vehicles/{id}/police-check`
3. VehicleService queries TrafficPoliceService
4. Police responds with vehicle status
5. Admin sees if vehicle is wanted or has violations
6. Decision: Approve or Reject registration

### Scenario 2: Reporting Vehicle to Police
1. Vehicle ownership transferred in VehicleService
2. VehicleService calls: `POST /api/vehicles/{id}/report-to-police`
3. VehicleService sends vehicle data to TrafficPoliceService
4. Police logs the report for records
5. Confirmation returned to VehicleService

## Testing

### Start the Services

**Terminal 1 - TrafficPoliceService**:
```bash
cd TrafficPoliceService
dotnet run
# Runs on http://localhost:5003
```

**Terminal 2 - VehicleService** (with existing background job):
```bash
cd VehicleService
dotnet run
# Runs on http://localhost:5001
```

### Test Police Service Directly

**Check if vehicle is wanted/has violations**:
```bash
curl http://localhost:5003/api/policevehicle/check-vehicle/BG-1234-AA
```

**Response**:
```json
{
  "message": "Vehicle check completed",
  "data": {
    "registrationNumber": "BG-1234-AA",
    "isWanted": false,
    "activeViolations": [...],
    "violationCount": 2,
    "outstandingFines": 50,
    "status": "Alert"
  }
}
```

**Get all wanted vehicles**:
```bash
curl http://localhost:5003/api/policevehicle/wanted-vehicles
```

**Get violations for a vehicle**:
```bash
curl http://localhost:5003/api/policevehicle/violations/BG-1234-AA
```

### Test Integration with VehicleService

**Check vehicle status with police**:
```bash
curl http://localhost:5001/api/vehicles/1/police-check
```

**Report vehicle to police**:
```bash
curl -X POST http://localhost:5001/api/vehicles/1/report-to-police
```

## Try It in Swagger

1. **TrafficPoliceService Swagger**: http://localhost:5003/swagger
   - See all police endpoints
   - Test them directly
   - View hardcoded data

2. **VehicleService Swagger**: http://localhost:5001/swagger
   - Find new `/api/vehicles/{id}/police-check` endpoint
   - Find new `/api/vehicles/{id}/report-to-police` endpoint
   - Call them to test integration

## Sample Test Flow

1. Start both services
2. Open Swagger: http://localhost:5003/swagger
3. Call: `GET /api/policevehicle/check-vehicle/BG-1234-AA`
   - See violations and fine amounts
4. Call: `GET /api/policevehicle/check-vehicle/BG-9999-XX`
   - See that vehicle is WANTED
5. Open: http://localhost:5001/swagger
6. Call: `GET /api/vehicles/1/police-check`
   - See combined response from both services

## Future Enhancements

- Database persistence for wanted vehicles/violations
- Real authentication between services
- License plate recognition integration
- Real-time alerts when wanted vehicle detected
- Integration with other agencies (Insurance, Ministry)
- Statistical reports on violations
- Payment processing for fines
- Automated blocking of vehicle operations if wanted

## Architecture Benefits

✅ **Shows Real-World Integration**: Not isolated - services collaborate
✅ **Realistic Workflow**: Police verify vehicles before operations
✅ **Hardcoded Data**: Easy to test without database setup
✅ **Extensible**: Ready to add database persistence
✅ **Demonstrates Inter-Service Communication**: Key microservice concept
✅ **Clear Separation of Concerns**: Police handles police data, Vehicle handles registration

## Files Created/Modified

**New Files**:
- `TrafficPoliceService/` - Complete new service
- `TrafficPoliceService/Models/` - Data models
- `TrafficPoliceService/Controllers/PoliceVehicleController.cs` - 6 endpoints
- `TrafficPoliceService/README.md` - Service documentation

**Modified Files**:
- `VehicleService/Program.cs` - Added TrafficPoliceService HttpClient
- `VehicleService/Controllers/VehiclesController.cs` - Added 2 new endpoints
- `eUpravaProject.sln` - Added TrafficPoliceService project

---

**Summary**: You now have a complete 4-service microservices system with realistic inter-agency collaboration! 🚗👮
