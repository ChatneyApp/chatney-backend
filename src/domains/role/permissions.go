package role

import "chatney-backend/src/domains/role/models"

const (
	deleteRole models.PermissionKey = "role.deleteRole"
	editRole   models.PermissionKey = "role.editRole"
	createRole models.PermissionKey = "role.createRole"
	readRole   models.PermissionKey = "role.readRole"
)

var RolePermissions = []models.PermissionKey{
	deleteRole, editRole, createRole, readRole,
}
