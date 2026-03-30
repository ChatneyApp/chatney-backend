using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    public async Task<Role> AddRole(PgRepo<Role, int> rolesRepo, RoleDto roleDto)
    {
        var role = Role.FromDto(roleDto);
        await rolesRepo.InsertOne(role);
        return role;
    }

    public async Task<Role> UpdateRole(PgRepo<Role, int> rolesRepo, Role role)
    {
        await rolesRepo.UpdateOne(role);
        return role;
    }

    public async Task<bool> DeleteRole(PgRepo<Role, int> rolesRepo, int id)
    {
        return await rolesRepo.DeleteById(id);
    }
}
