package models

import (
	"time"

	"gorm.io/gorm"
)

type StolenVehicle struct {
	gorm.Model
	// SQL Server requires fixed length for Unique Indexes
	VehiclePlate string    `gorm:"type:nvarchar(20);uniqueIndex;not null" json:"vehiclePlate"`
	ReportedDate time.Time `json:"reportedDate"`
	Description  string    `gorm:"type:nvarchar(max)" json:"description"`
	Status       string    `gorm:"type:nvarchar(20)" json:"status"` // ACTIVE, RECOVERED
	ContactInfo  string    `gorm:"type:nvarchar(255)" json:"contactInfo"`
}
