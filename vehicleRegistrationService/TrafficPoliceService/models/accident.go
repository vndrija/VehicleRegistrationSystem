package models

import (
	"time"

	"gorm.io/gorm"
)

type Accident struct {
	gorm.Model
	Location       string    `json:"location"`
	Description    string    `json:"description"`
	Severity       string    `json:"severity"` // MINOR, MAJOR, CRITICAL
	AccidentDate   time.Time `json:"accidentDate"`
	InvolvedPlates string    `json:"involvedPlates"` // Comma-separated plates for simplicity
	IsResolved     bool      `json:"isResolved"`
}
