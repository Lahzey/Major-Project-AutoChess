using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MPAutoChess.logic.core.networking;

public interface IIdentifiable {

    private const int CLEANUP_INTERVAL_MS = 60000; // 60 seconds
    
    // WeakReference to allow for garbage collection of instances (otherwise they would stay in memory forever)
    private static Dictionary<string, WeakReference<IIdentifiable>> instances = new Dictionary<string, WeakReference<IIdentifiable>>();

    private static long lastCleanup = 0;

    public static IIdentifiable? TryGetInstance(string id) {
        if (instances.TryGetValue(id, out WeakReference<IIdentifiable> weakRef)) {
            if (weakRef.TryGetTarget(out IIdentifiable instance)) {
                return instance;
            } else {
                instances.Remove(id); // do some immediate cleanup (not necessary)
                return null;
            }
        }
        return null;
    }

    public static void CheckForCleanup() {
        long now = Environment.TickCount; // do not use DateTime.Now here, it is very slow
        if (now < lastCleanup + CLEANUP_INTERVAL_MS) {
            return;
        }
        
        foreach (string key in new List<string>(instances.Keys)) {
            if (!instances[key].TryGetTarget(out _)) {
                instances.Remove(key);
            }
        }

        lastCleanup = now;
    }
    
    private static void RegisterInstance(IIdentifiable instance) {
        if (string.IsNullOrEmpty(instance.Id)) {
            instance.Id = Guid.NewGuid().ToString();
        }

        WeakReference<IIdentifiable> weakRef = new WeakReference<IIdentifiable>(instance, true);
        instances.Add(instance.Id, weakRef);
    }
    
    /// <summary>
    /// Removes the instance from the registry. This is done automatically when the instance is garbage collected, but can be used to manually unregister an instance.
    /// Useful for testing serialization and deserialization on the same machine as if they were on different machines.
    /// </summary>
    /// <param name="instance">The instance to remove.</param>
    public static void UnregisterInstance(IIdentifiable instance) {
        instances.Remove(instance.Id);
        instance.Id = null;
    }
    
    public void SetId(string id) {
        if (Id != null && Id != id) {
            throw new ArgumentException("IIdentifiable already registered under a different ID.");
        }
        Id = id;
        RegisterInstance(this);
    }

    public string GetId() {
        if (Id == null) {
            Id = Guid.NewGuid().ToString();
            RegisterInstance(this);
        }
        return Id;
    }

    protected string Id { get; set; }
}