using System;
using System.Collections.Generic;

public static class Services
{
    private static readonly Dictionary<Type, IService> _services = new();

    public static bool IsInitialized { get; private set; }

    public static void Register(IService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        Type type = service.GetType();
        if (_services.ContainsKey(type))
            throw new InvalidOperationException($"Service of type '{type.Name}' is already registered.");

        _services[type] = service;
    }

    public static T Get<T>() where T : class, IService
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Services have not been initialized yet.");

        if (_services.TryGetValue(typeof(T), out IService service))
            return (T)service;

        throw new InvalidOperationException($"Service of type '{typeof(T).Name}' is not registered.");
    }

    public static bool TryGet<T>(out T service) where T : class, IService
    {
        service = null;

        if (!IsInitialized)
            return false;

        if (_services.TryGetValue(typeof(T), out IService found))
        {
            service = (T)found;
            return true;
        }

        return false;
    }

    internal static void MarkInitialized()
    {
        IsInitialized = true;
    }

    internal static IReadOnlyCollection<IService> GetAll()
    {
        return _services.Values;
    }
}
