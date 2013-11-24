// -----------------------------------------------------------------------
// <copyright file="TransientDataStorage.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------


using Microsoft.Phone.Shell;


public class TransientDataStorage : IDataStorage
{

    public bool Backup(string token, object value)
    {
        if (null == value)
            return false;

        var store = PhoneApplicationService.Current.State;
        if (store.ContainsKey(token))
            store[token] = value;
        else
            store.Add(token, value);

        return true;
    }

    public T Restore<T>(string token)
    {
        var store = PhoneApplicationService.Current.State;
        if (!store.ContainsKey(token))
            return default(T);

        return (T)store[token];
    }
}


public interface IDataStorage
{
    bool Backup(string token, object value);
    T Restore<T>(string token);
}


