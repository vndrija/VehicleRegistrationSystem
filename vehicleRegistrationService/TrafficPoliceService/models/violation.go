package models

import (
	"time"

	"gorm.io/gorm"
)

type Violation struct {
	gorm.Model
	// Index added for fast lookups by plate
	VehiclePlate  string    `gorm:"type:nvarchar(20);index;not null" json:"vehiclePlate"`
	OfficerID     uint      `json:"officerId"`
	Description   string    `gorm:"type:nvarchar(max)" json:"description"`
	Location      string    `gorm:"type:nvarchar(255)" json:"location"`
	FineAmount    float64   `json:"fineAmount"`
	Status        string    `gorm:"type:nvarchar(20)" json:"status"` // PENDING, PAID, DISMISSED
	ViolationDate time.Time `json:"violationDate"`
}
