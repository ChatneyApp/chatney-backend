package workspace

import (
	"chatney-backend/src/domains/role/models"
)

const (
	CreateWorkspace models.PermissionKey = "workspace.createWorkspace"
	DeleteWorkspace models.PermissionKey = "workspace.deleteWorkspace"
	UpdateWorkspace models.PermissionKey = "workspace.updateWorkspace"
	ReadWorkspace   models.PermissionKey = "workspace.readWorkspace"
)

// Слайс со всеми возможными значениями PermissionKey
var WorkspacePermissions = []models.PermissionKey{
	CreateWorkspace,
	DeleteWorkspace,
	UpdateWorkspace,
	ReadWorkspace,
}
