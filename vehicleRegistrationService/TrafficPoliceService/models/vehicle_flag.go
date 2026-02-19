package models

import "gorm.io/gorm"

// Flags like "Expired Registration", "Warrant", etc.
type VehicleFlag struct {
	gorm.Model
	VehiclePlate string `json:"vehiclePlate"`
	FlagType     string `json:"flagType"`
	Description  string `json:"description"`
	IsActive     bool   `json:"isActive"`
}
