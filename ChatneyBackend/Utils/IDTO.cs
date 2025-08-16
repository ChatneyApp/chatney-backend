using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Utils;

interface IDTO<T>
{
    public T ToModel();
}