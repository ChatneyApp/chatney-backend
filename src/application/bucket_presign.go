package application

import (
	"context"
	"net/url"
	"time"
)

// GetPresignedURL returns a presigned URL for the given object key and expiry duration
func (bucket *Bucket) GetPresignedURL(key string, expiry time.Duration) (string, error) {
	reqParams := make(url.Values)
	presignedURL, err := bucket.Client.PresignedGetObject(context.Background(), bucket.bucketName, key, expiry, reqParams)
	if err != nil {
		return "", err
	}
	return presignedURL.String(), nil
}
