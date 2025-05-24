package models

import (
	"time"
)

type Reaction struct {
	UserId   string `json:"userId"`
	Reaction string `json:"reaction"`
}

type Message struct {
	Id          string     `json:"_id"`
	ChannelId   string     `json:"channelId"`
	UserId      string     `json:"userId"`
	Content     string     `json:"content"`
	Attachments []string   `json:"attachments"`
	Status      string     `json:"status"`
	CreatedAt   time.Time  `json:"createdAt"`
	UpdatedAt   time.Time  `json:"updatedAt"`
	Reactions   []Reaction `json:"reactions"`
	ParentId    string     `json:"parentId"`
}
