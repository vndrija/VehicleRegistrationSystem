package services

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"trafficpolice/config"
)

// NotifyVehicleService sends status updates to the C# microservice
func NotifyVehicleService(plate string, status string, cfg *config.Config) error {
	// Payload matching what the C# service likely expects
	payload := map[string]string{
		"vehiclePlate": plate,
		"status":       status, // e.g., "STOLEN", "ACCIDENT", "CLEAR"
	}
	jsonPayload, _ := json.Marshal(payload)

	url := fmt.Sprintf("%s/api/vehicles/update-status", cfg.VehicleServiceUrl)

	resp, err := http.Post(url, "application/json", bytes.NewBuffer(jsonPayload))
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	if resp.StatusCode >= 400 {
		return fmt.Errorf("vehicle service returned status: %d", resp.StatusCode)
	}

	return nil
}
