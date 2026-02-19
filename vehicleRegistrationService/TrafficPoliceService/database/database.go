package database

import (
	"log"
	"trafficpolice/models"

	"gorm.io/driver/sqlserver"
	"gorm.io/gorm"
)

var DB *gorm.DB

func Connect(connectionString string) {
	var err error
	// GORM will automatically attempt to connect.
	// Note: The 'TrafficPoliceDb' database must usually exist in SQL Server.
	// If it doesn't, GORM might fail. For this project, we assume the DB is created
	// or we rely on SQL Server's ability to handle the connection string.
	DB, err = gorm.Open(sqlserver.Open(connectionString), &gorm.Config{})
	if err != nil {
		log.Fatal("Failed to connect to database:", err)
	}

	log.Println("Connected to SQL Server successfully")

	// AutoMigrate creates the tables based on our models
	err = DB.AutoMigrate(
		&models.Officer{},
		&models.Violation{},
		&models.Accident{},
		&models.StolenVehicle{},
		&models.VehicleFlag{},
	)
	if err != nil {
		log.Fatal("Failed to migrate database:", err)
	}
	log.Println("Database migration completed")
}
