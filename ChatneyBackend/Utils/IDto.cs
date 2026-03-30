using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Utils;

interface IDto<T>
{
    T ToModel();
}
