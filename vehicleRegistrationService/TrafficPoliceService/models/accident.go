package models

import (
	"time"

	"gorm.io/gorm"
)

type Accident struct {
	gorm.Model
	Location       string    `gorm:"type:nvarchar(255)" json:"location"`
	Description    string    `gorm:"type:nvarchar(max)" json:"description"`
	Severity       string    `gorm:"type:nvarchar(20)" json:"severity"` // MINOR, MAJOR, CRITICAL
	AccidentDate   time.Time `json:"accidentDate"`
	InvolvedPlates string    `gorm:"type:nvarchar(500)" json:"involvedPlates"` // CSV of plates
	IsResolved     bool      `json:"isResolved"`
}
