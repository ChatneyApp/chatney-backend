package role

import "chatney-backend/src/domains/role/models"

const (
	DeleteRole models.PermissionKey = "role.deleteRole"
	EditRole   models.PermissionKey = "role.editRole"
	CreateRole models.PermissionKey = "role.createRole"
	ReadRole   models.PermissionKey = "role.readRole"
)

var RolePermissions = []models.PermissionKey{
	DeleteRole, EditRole, CreateRole, ReadRole,
}
