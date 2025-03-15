package workspace

import (
	"chatney-backend/src/domains/role/models"
)

const (
	createWorkspace models.PermissionKey = "workspace.createWorkspace"
	deleteWorkspace models.PermissionKey = "workspace.deleteWorkspace"
	updateWorkspace models.PermissionKey = "workspace.updateWorkspace"
	readWorkspace   models.PermissionKey = "workspace.readWorkspace"
)

var WorkspacePermissions = []models.PermissionKey{
	createWorkspace,
	deleteWorkspace,
	updateWorkspace,
	readWorkspace,
}
