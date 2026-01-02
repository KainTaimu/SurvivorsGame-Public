using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace Game.Core.Services;

public partial class ServiceLocator : Node
{
    [Export]
    private Array<Service> _exportedServices = null!;

    private static readonly List<Service> _services = [];

    public static ServiceLocator Instance { get; private set; } = null!;

    public override void _Ready()
    {
        if (_services.Count > 0)
        {
            Logger.LogError(
                "_services is not empty when it shouldn't. ServiceLocator should only ever have one instance per launch. Clearing previous entries"
            );
            _services.Clear();
        }
        foreach (var service in _exportedServices)
            _services.Add(service);

        Instance = this;
    }

    public static T? GetService<T>()
        where T : Service
    {
        var service = _services.OfType<T>().FirstOrDefault();
        if (service is null)
        {
            Logger.LogError($"Service \"{typeof(T).Name}\" not found!");
            return null;
        }

        return service;
    }
}
