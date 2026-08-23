using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public static class ServiceBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        DiscoverAndRegister();
        InitializeAll();
    }

    private static void DiscoverAndRegister()
    {
        IEnumerable<Type> serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(t => typeof(IService).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract
                        && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (Type type in serviceTypes)
        {
            IService instance = (IService)Activator.CreateInstance(type);
            Services.Register(instance);
            Debug.Log($"[Services] Registered {type.Name}");
        }
    }

    private static async void InitializeAll()
    {
        IReadOnlyCollection<IService> all = Services.GetAll();
        List<Task> tasks = new(all.Count);

        foreach (IService service in all)
            tasks.Add(service.InitializeAsync());

        try
        {
            await Task.WhenAll(tasks);
            Services.MarkInitialized();
            Debug.Log($"[Services] Initialized {all.Count} service(s).");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Services] Initialization failed: {ex}");
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null);
        }
    }
}
