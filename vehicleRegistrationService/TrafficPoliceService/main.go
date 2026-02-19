package main

import (
	"trafficpolice/config"
	"trafficpolice/database"
	"trafficpolice/handlers"
	"trafficpolice/middleware"

	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"
)

func main() {
	// 1. Load Config
	cfg := config.LoadConfig()

	// 2. Connect Database
	database.Connect(cfg.DBUrl)

	// 3. Setup Router
	r := gin.Default()

	// 4. CORS Setup (Allow Angular frontend)
	r.Use(cors.New(cors.Config{
		AllowOrigins:     []string{"http://localhost:4200"}, // Angular URL
		AllowMethods:     []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowHeaders:     []string{"Origin", "Content-Type", "Authorization"},
		ExposeHeaders:    []string{"Content-Length"},
		AllowCredentials: true,
	}))

	// 5. Routes
	api := r.Group("/api/police")

	// Apply Auth Middleware to all routes (or specific ones)
	api.Use(middleware.AuthMiddleware(cfg))
	{
		// Stolen Vehicles
		api.POST("/stolen", handlers.ReportStolen)
		api.GET("/stolen", handlers.GetStolenVehicles)

		// Add other handlers here (Violations, Accidents, Officers)
		// api.POST("/violations", handlers.CreateViolation)
	}

	// 6. Run
	r.Run(":" + cfg.Port) // Listen on port 8080 (internal Docker port)
}
