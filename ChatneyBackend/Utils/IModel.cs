namespace ChatneyBackend.Utils;

interface IModel<T>
{
    T ToResponse();
}