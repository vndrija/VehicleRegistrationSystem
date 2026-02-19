package models

import (
	"time"

	"gorm.io/gorm"
)

type StolenVehicle struct {
	gorm.Model
	VehiclePlate string    `gorm:"uniqueIndex;not null" json:"vehiclePlate"`
	ReportedDate time.Time `json:"reportedDate"`
	Description  string    `json:"description"`
	Status       string    `json:"status"` // ACTIVE, RECOVERED
	ContactInfo  string    `json:"contactInfo"`
}
