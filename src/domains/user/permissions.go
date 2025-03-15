package user

import "chatney-backend/src/domains/role/models"

const (
	deleteUser models.PermissionKey = "user.deleteUser"
	editUser   models.PermissionKey = "user.editUser"
	createUser models.PermissionKey = "user.createUser"
	readUser   models.PermissionKey = "user.readUser"
)

var UserPermissions = []models.PermissionKey{
	deleteUser, editUser, createUser, readUser,
}
