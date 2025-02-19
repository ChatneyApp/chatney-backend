package application

import (
	"context"
	"fmt"
	"log"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"
)

type Bucket struct {
	Client     *minio.Client
	bucketName string
}

type PutResult struct {
	Key  string `json:"key"`
	Size int    `json:"size"`
}

func (bucket *Bucket) PutFile(key string, localPath string, contentType string) PutResult {
	uploadInfo, err := bucket.Client.FPutObject(
		context.Background(),
		bucket.bucketName,
		key,
		localPath,
		minio.PutObjectOptions{ContentType: contentType},
	)

	if err != nil {
		log.Fatalln("Error uploading file:", err)
	}

	return PutResult{
		Key:  uploadInfo.Key,
		Size: int(uploadInfo.Size),
	}
}

func NewBucketConnection(config *EnvConfig) *Bucket {
	// Создаём клиент MinIO
	minioClient, err := minio.New(config.BucketHost, &minio.Options{
		Creds:  credentials.NewStaticV4(config.BucketAccessKeyID, config.BucketSecretAccessKey, ""),
		Secure: config.BucketUseSSL,
	})
	if err != nil {
		log.Fatalln("Error connecting to MinIO:", err)
	}

	fmt.Println("✅ Successful MinIO connection!")

	// Create new bucket if not exists

	ctx := context.Background()
	err = minioClient.MakeBucket(ctx, config.BucketName, minio.MakeBucketOptions{Region: config.BucketRegion})
	if err != nil {
		// Проверяем, есть ли уже бакет
		exists, errBucketExists := minioClient.BucketExists(ctx, config.BucketName)
		if errBucketExists == nil && exists {
			fmt.Println("✅ Bucket already exists:", config.BucketName)
		} else {
			log.Fatalln("Error creating bucket:", err)
		}
	} else {
		fmt.Println("✅ Bucket created:", config.BucketName)
	}

	return &Bucket{
		Client:     minioClient,
		bucketName: config.BucketName,
	}
}
