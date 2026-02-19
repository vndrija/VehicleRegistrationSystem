package models

import (
	"time"

	"gorm.io/gorm"
)

type Violation struct {
	gorm.Model
	VehiclePlate  string    `json:"vehiclePlate"`
	OfficerID     uint      `json:"officerId"` // Foreign key
	Description   string    `json:"description"`
	Location      string    `json:"location"`
	FineAmount    float64   `json:"fineAmount"`
	Status        string    `json:"status"` // PENDING, PAID, DISMISSED
	ViolationDate time.Time `json:"violationDate"`
}
