package application

import (
	"fmt"
	"os"
	"reflect"
	"strconv"
)

type EnvConfig struct {
	MongoConnectionUri    string
	MongoDbName           string
	BucketHost            string
	BucketAccessKeyID     string
	BucketSecretAccessKey string
	BucketUseSSL          bool
	BucketName            string
	BucketRegion          string
	ApiPort               string
	PasswordSalt          string
	JwtKey                string
}

var Config = LoadEnvConfig()

func getEnvWithDefault(key, defaultValue string) string {
	if val, exists := os.LookupEnv(key); exists {
		return val
	}
	return defaultValue
}

func parseBoolEnv(key string) bool {
	val := getEnvWithDefault(key, "false")
	parsed, err := strconv.ParseBool(val)
	if err != nil {
		return false
	}
	return parsed
}

func LoadEnvConfig() *EnvConfig {
	config := &EnvConfig{
		MongoConnectionUri:    getEnvWithDefault("MONGO_CONNECTION_URI", ""),
		MongoDbName:           getEnvWithDefault("MONGO_DB_NAME", ""),
		BucketHost:            getEnvWithDefault("BUCKET_HOST", ""),
		BucketAccessKeyID:     getEnvWithDefault("BUCKET_ACCESS_KEY_ID", ""),
		BucketSecretAccessKey: getEnvWithDefault("BUCKET_SECRET_ACCESS_KEY", ""),
		BucketUseSSL:          parseBoolEnv("BUCKET_USE_SSL"),
		BucketName:            getEnvWithDefault("BUCKET_NAME", ""),
		BucketRegion:          getEnvWithDefault("BUCKET_REGION", ""),
		ApiPort:               getEnvWithDefault("API_PORT", ""),
		PasswordSalt:          getEnvWithDefault("PASSWORD_SALT", ""),
		JwtKey:                getEnvWithDefault("JWT_KEY", ""),
	}

	if err := validateStruct(config); err != nil {
		panic("Error loading env vars")
	}

	return config
}

func validateStruct(s interface{}) error {
	v := reflect.ValueOf(s).Elem()

	for i := 0; i < v.NumField(); i++ {
		field := v.Field(i)
		fieldName := v.Type().Field(i).Name

		// Проверяем строковые поля на пустоту
		if field.Kind() == reflect.String && field.String() == "" {
			return fmt.Errorf("❌ Field %s is not set", fieldName)
		}
	}

	return nil
}
