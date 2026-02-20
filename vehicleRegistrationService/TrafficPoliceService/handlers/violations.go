package handlers

import (
	"net/http"
	"time"
	"trafficpolice/database"
	"trafficpolice/models"

	"github.com/gin-gonic/gin"
)

// IssueViolation creates a new traffic ticket.
func IssueViolation(c *gin.Context) {
	var input models.Violation

	// 1. Bind input
	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// 2. Set defaults if not provided by the frontend.
	// In Go, time.Time zero value is messy, so usually better to set Now() explicitly.
	if input.ViolationDate.IsZero() {
		input.ViolationDate = time.Now()
	}
	// Default status is usually PENDING payment
	if input.Status == "" {
		input.Status = "PENDING"
	}

	// 3. Save to DB
	if err := database.DB.Create(&input).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to issue violation"})
		return
	}

	c.JSON(http.StatusCreated, input)
}

// GetViolationsByPlate returns all tickets for a specific car.
// Useful for the "Vehicle Status" check.
func GetViolationsByPlate(c *gin.Context) {
	plate := c.Param("plate") // e.g., /violations/BG-123-XX

	var violations []models.Violation

	// Find all records matching the plate.
	// Unlike 'First', 'Find' does NOT return an error if the list is empty;
	// it just returns an empty slice, which is correct (0 violations is valid).
	result := database.DB.Where("vehicle_plate = ?", plate).Find(&violations)

	if result.Error != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": result.Error.Error()})
		return
	}

	c.JSON(http.StatusOK, violations)
}

// PayViolation updates the status of a ticket to PAID.
func PayViolation(c *gin.Context) {
	id := c.Param("id") // The primary key ID of the violation

	var violation models.Violation

	// 1. Find the violation first
	if err := database.DB.First(&violation, id).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Violation not found"})
		return
	}

	// 2. Update the status field
	violation.Status = "PAID"
	database.DB.Save(&violation)

	c.JSON(http.StatusOK, gin.H{"message": "Violation paid successfully", "data": violation})
}
