using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    public async Task<Role> AddRole(AppRepos repos, RoleDto roleDto)
    {
        var role = Role.FromDto(roleDto);
        await repos.Roles.InsertOne(role);
        return role;
    }

    public async Task<Role> UpdateRole(AppRepos repos, Role role)
    {
        await repos.Roles.UpdateOne(role);
        return role;
    }

    public async Task<bool> DeleteRole(AppRepos repos, int id)
    {
        return await repos.Roles.DeleteById(id);
    }
}
