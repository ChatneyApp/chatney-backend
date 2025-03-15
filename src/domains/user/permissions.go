package user

import "chatney-backend/src/domains/role/models"

const (
	DeleteUser models.PermissionKey = "user.deleteUser"
	EditUser   models.PermissionKey = "user.editUser"
	CreateUser models.PermissionKey = "user.createUser"
	ReadUser   models.PermissionKey = "user.readUser"
)

var UserPermissions = []models.PermissionKey{
	DeleteUser, EditUser, CreateUser, ReadUser,
}
