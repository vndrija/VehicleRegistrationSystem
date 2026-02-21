// package main

// import (
// 	"fmt"
// 	"time"

// 	"github.com/golang-jwt/jwt/v5"
// )

// func main() {
// 	// Settings from your docker-compose
// 	secret := []byte("YourSuperSecretKeyForJWT_MustBe32CharactersOrMore!")

// 	claims := jwt.MapClaims{
// 		"iss":  "eUpravaAuthService",
// 		"sub":  "test-admin-id", // Fake UserID
// 		"role": "PoliceAdmin",   // Fake Role
// 		"exp":  time.Now().Add(time.Hour * 24).Unix(),
// 	}

// 	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
// 	tokenString, _ := token.SignedString(secret)

// 	fmt.Println("\n⬇️  COPY THIS TOKEN  ⬇️")
// 	fmt.Println(tokenString)
// 	fmt.Println("⬆️  USE IN AUTHORIZATION HEADER  ⬆️\n")
// }