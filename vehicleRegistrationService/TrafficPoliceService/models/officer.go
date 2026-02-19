package models

import "gorm.io/gorm"

type Officer struct {
	gorm.Model
	BadgeNumber string `gorm:"uniqueIndex;not null" json:"badgeNumber"`
	FirstName   string `json:"firstName"`
	LastName    string `json:"lastName"`
	Rank        string `json:"rank"`
	StationID   string `json:"stationId"`
	UserID      string `json:"userId"` // Link to Auth service UserID if needed
}
