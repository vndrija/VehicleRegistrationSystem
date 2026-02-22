package handlers

import (
	"net/http"
	"strings"
	"trafficpolice/config"
	"trafficpolice/services"

	"github.com/gin-gonic/gin"
	// ...
)

func ProxyVehicleDetails(c *gin.Context) {
	plate := c.Param("plate")
	cfg := config.LoadConfig()

	// 1. Extract the Token from the "Authorization" header
	authHeader := c.GetHeader("Authorization")
	if authHeader == "" {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "Authorization header missing"})
		return
	}
	// Remove "Bearer " prefix if present
	token := strings.TrimPrefix(authHeader, "Bearer ")

	// 2. Pass token to the service
	vehicleData, err := services.GetVehicleDetails(plate, token, cfg)

	if err != nil {
		// Log specific error for debugging
		if strings.Contains(err.Error(), "401") {
			c.JSON(http.StatusBadGateway, gin.H{"error": "Vehicle Registry rejected our credentials (401)"})
		} else {
			c.JSON(http.StatusBadGateway, gin.H{"error": "Failed to communicate with Vehicle Registry", "details": err.Error()})
		}
		return
	}

	if vehicleData == nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Vehicle not found in registry"})
		return
	}

	c.JSON(http.StatusOK, vehicleData)
}
