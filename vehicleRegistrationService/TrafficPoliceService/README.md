# Traffic Police Service

A microservice representing Traffic Police collaboration in the eUprava Vehicle Registration System. Manages wanted vehicles, traffic violations, and vehicle status verification.

## Overview

This service demonstrates inter-agency collaboration by:
- Maintaining a database of wanted vehicles
- Tracking traffic violations
- Providing vehicle status checks to VehicleService
- Receiving vehicle reports from VehicleService

## Architecture

- **Port**: 5003
- **Framework**: ASP.NET Core 10 (.NET 10)
- **Type**: RESTful API
- **Data**: Hardcoded (in-memory database)

## API Endpoints

### 1. Check Vehicle Status with Police
**GET** `/api/policevehicle/check-vehicle/{registrationNumber}`

Checks if a vehicle is wanted or has traffic violations.

**Response**:
```json
{
  "message": "Vehicle check completed",
  "data": {
    "registrationNumber": "BG-1234-AA",
    "isWanted": false,
    "wantedReason": null,
    "activeViolations": [
      {
        "id": 1,
        "registrationNumber": "BG-1234-AA",
        "ownerName": "Marko Marković",
        "violationType": "Speeding (Over 50km/h limit)",
        "location": "Knez Mihailova Street, Belgrade",
        "dateOfViolation": "2026-01-24",
        "fineAmount": 50,
        "status": "Pending"
      }
    ],
    "violationCount": 2,
    "outstandingFines": 50,
    "status": "Alert",
    "checkedAt": "2026-02-08T17:40:00"
  }
}
```

### 2. Get Wanted Vehicles
**GET** `/api/policevehicle/wanted-vehicles`

Retrieves all active wanted vehicles.

**Response**:
```json
{
  "message": "Wanted vehicles retrieved",
  "count": 2,
  "data": [
    {
      "id": 1,
      "registrationNumber": "BG-9999-XX",
      "make": "BMW",
      "model": "X5",
      "year": 2020,
      "color": "Black",
      "reason": "Vehicle used in organized crime",
      "dateReported": "2025-08-08",
      "status": "Active"
    }
  ]
}
```

### 3. Get Vehicle Violations
**GET** `/api/policevehicle/violations/{registrationNumber}`

Retrieves all traffic violations for a specific vehicle.

**Response**:
```json
{
  "message": "Traffic violations retrieved",
  "count": 2,
  "data": [
    {
      "id": 1,
      "registrationNumber": "BG-1234-AA",
      "ownerName": "Marko Marković",
      "violationType": "Speeding (Over 50km/h limit)",
      "location": "Knez Mihailova Street, Belgrade",
      "dateOfViolation": "2026-01-24",
      "fineAmount": 50,
      "status": "Pending"
    }
  ]
}
```

### 4. Report Vehicle to Police
**POST** `/api/policevehicle/report-vehicle`

VehicleService sends vehicle data to police for logging/verification.

**Request Body**:
```json
{
  "registrationNumber": "BG-1234-AA",
  "ownerName": "Marko Marković",
  "make": "Toyota",
  "model": "Corolla",
  "expirationDate": "2027-02-08"
}
```

**Response**:
```json
{
  "message": "Vehicle report received and logged",
  "data": {
    "registrationNumber": "BG-1234-AA",
    "ownerName": "Marko Marković",
    "make": "Toyota",
    "model": "Corolla",
    "expirationDate": "2027-02-08",
    "receivedAt": "2026-02-08T17:40:00",
    "status": "Logged"
  }
}
```

### 5. Add Wanted Vehicle
**POST** `/api/policevehicle/wanted-vehicles`

Police officer endpoint to add a vehicle to the wanted list.

**Request Body**:
```json
{
  "registrationNumber": "BG-8888-YY",
  "make": "Audi",
  "model": "A6",
  "year": 2021,
  "color": "Red",
  "reason": "Vehicle involved in hit-and-run"
}
```

### 6. Record Traffic Violation
**POST** `/api/policevehicle/violations`

Police officer endpoint to record a traffic violation.

**Request Body**:
```json
{
  "registrationNumber": "BG-1234-AA",
  "ownerName": "Marko Marković",
  "violationType": "Parking in disabled zone",
  "location": "Terazije, Belgrade",
  "fineAmount": 75
}
```

## Integration with VehicleService

### Endpoint 1: Check Vehicle with Police
**GET** `http://localhost:5001/api/vehicles/{id}/police-check`

VehicleService queries police database to verify vehicle status before approving operations.

**Usage**:
```bash
GET http://localhost:5001/api/vehicles/1/police-check
```

**Response**:
```json
{
  "message": "Police verification completed",
  "vehicle": {
    "id": 1,
    "registrationNumber": "BG-1234-AA",
    "make": "Toyota",
    "model": "Corolla",
    "ownerName": "Marko Marković"
  },
  "policeReport": {
    "registrationNumber": "BG-1234-AA",
    "isWanted": false,
    "activeViolations": [],
    "violationCount": 1,
    "outstandingFines": 50,
    "status": "Alert"
  }
}
```

### Endpoint 2: Report Vehicle to Police
**POST** `http://localhost:5001/api/vehicles/{id}/report-to-police`

VehicleService reports vehicle information to police when transferring, registering, or managing vehicles.

**Usage**:
```bash
POST http://localhost:5001/api/vehicles/1/report-to-police
```

## Data Model

### WantedVehicle
```csharp
{
  Id,
  RegistrationNumber,
  Make,
  Model,
  Year,
  Color,
  Reason,
  DateReported,
  Status
}
```

### TrafficViolation
```csharp
{
  Id,
  RegistrationNumber,
  OwnerName,
  ViolationType,
  Location,
  DateOfViolation,
  FineAmount,
  Status
}
```

### PoliceVehicleReport
```csharp
{
  RegistrationNumber,
  IsWanted,
  WantedReason,
  ActiveViolations,
  ViolationCount,
  OutstandingFines,
  Status,
  CheckedAt
}
```

## Hardcoded Test Data

### Sample Wanted Vehicles
- **BG-9999-XX**: BMW X5 - Used in organized crime
- **NS-1111-AA**: Mercedes-Benz E-Class - Stolen vehicle

### Sample Traffic Violations
- **BG-1234-AA** (Marko Marković): Speeding - 50€
- **BG-1234-AA** (Marko Marković): Expired Registration - 100€ (Paid)
- **NS-5678-BB** (Ana Anić): No Parking Zone - 30€

## Running the Service

```bash
cd TrafficPoliceService
dotnet run

# Service runs on http://localhost:5003
# Swagger UI: http://localhost:5003/swagger
```

## Testing

### Test with curl

```bash
# Check if vehicle is wanted
curl http://localhost:5003/api/policevehicle/check-vehicle/BG-1234-AA

# Get all wanted vehicles
curl http://localhost:5003/api/policevehicle/wanted-vehicles

# Get violations for vehicle
curl http://localhost:5003/api/policevehicle/violations/BG-1234-AA

# Report vehicle to police
curl -X POST http://localhost:5003/api/policevehicle/report-vehicle \
  -H "Content-Type: application/json" \
  -d '{"registrationNumber":"BG-1234-AA","ownerName":"Marko","make":"Toyota","model":"Corolla","expirationDate":"2027-02-08"}'
```

### Test from VehicleService

```bash
# Check vehicle status with police
curl http://localhost:5001/api/vehicles/1/police-check

# Report vehicle to police
curl -X POST http://localhost:5001/api/vehicles/1/report-to-police
```

## Future Enhancements

- Database persistence (SQL Server)
- Authentication/Authorization
- Digital signature verification
- Audit logging
- Real-time alerts for wanted vehicles
- Integration with other government agencies
- Advanced filtering and search
