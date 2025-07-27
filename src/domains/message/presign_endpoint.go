package message

import (
	"chatney-backend/src/application"
	"net/http"
	"time"
)

// Handler for getting presigned URL for an attachment
func GetPresignedURLHandler(bucket *application.Bucket) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Query().Get("key")
		if key == "" {
			w.WriteHeader(http.StatusBadRequest)
			//nolint
			w.Write([]byte("missing key parameter"))
			return
		}
		url, err := bucket.GetPresignedURL(key, 15*time.Minute)
		if err != nil {
			w.WriteHeader(http.StatusInternalServerError)
			//nolint
			w.Write([]byte("failed to get presigned url"))
			return
		}
		//nolint
		w.Write([]byte(url))
	}
}
