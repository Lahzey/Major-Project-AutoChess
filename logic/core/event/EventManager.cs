using System;
using System.Collections.Generic;

namespace MPAutoChess.logic.core.@event;

public class EventManager {

    private readonly Dictionary<Type, EventTypeHandler> handlers = new Dictionary<Type, EventTypeHandler>(); // handlers will be initialized on demand
    
    public bool NotifyBefore<T>(T e) where T : Event {
        return GetEventTypeHandler<T>().Before(e);
    }

    public void NotifyAfter<T>(T e) where T : Event {
        GetEventTypeHandler<T>().After(e);
    }
    
    public void AddBeforeListener<T>(Action<T> listener, int priority = 0) where T : Event {
        EventTypeHandler<T> handler = GetEventTypeHandler<T>();
        handler.beforeListeners.Add(listener);
        handler.beforePriorities.Add(priority);
        handler.SortBeforeListeners();
    }
    
    public void AddAfterListener<T>(Action<T> listener, int priority = 0) where T : Event {
        EventTypeHandler<T> handler = GetEventTypeHandler<T>();
        handler.afterListeners.Add(listener);
        handler.afterPriorities.Add(priority);
        handler.SortAfterListeners();
    }
    
    public bool RemoveBeforeListener<T>(Action<T> listener) where T : Event {
        EventTypeHandler<T> handler = GetEventTypeHandler<T>();
        int index = handler.beforeListeners.IndexOf(listener);
        if (index >= 0) {
            handler.beforeListeners.RemoveAt(index);
            handler.beforePriorities.RemoveAt(index);
            return true;
        }
        return false;
    }
    
    public bool RemoveAfterListener<T>(Action<T> listener) where T : Event {
        EventTypeHandler<T> handler = GetEventTypeHandler<T>();
        int index = handler.afterListeners.IndexOf(listener);
        if (index >= 0) {
            handler.afterListeners.RemoveAt(index);
            handler.afterPriorities.RemoveAt(index);
            return true;
        }
        return false;
    }
    
    private EventTypeHandler<T> GetEventTypeHandler<T>() where T : Event {
        if (!handlers.ContainsKey(typeof(T))) {
            handlers[typeof(T)] = new EventTypeHandler<T>();
        }
        return (EventTypeHandler<T>)handlers[typeof(T)];
    }

    private interface EventTypeHandler {
        // workaround because C# does not support generic wildcards (? in Java), so you need to use a base type instead
    }

    private class EventTypeHandler<T> : EventTypeHandler where T : Event {
        public readonly List<Action<T>> beforeListeners = new List<Action<T>>();
        public readonly List<int> beforePriorities = new List<int>();
        public readonly List<Action<T>> afterListeners = new List<Action<T>>();
        public readonly List<int> afterPriorities = new List<int>();
        
        public void SortBeforeListeners() {
            beforeListeners.Sort((a, b) => beforePriorities[beforeListeners.IndexOf(a)].CompareTo(beforePriorities[beforeListeners.IndexOf(b)]));
        }
        
        public void SortAfterListeners() {
            afterListeners.Sort((a, b) => afterPriorities[afterListeners.IndexOf(a)].CompareTo(afterPriorities[afterListeners.IndexOf(b)]));
        }

        public bool Before(T e) {
            foreach (var listener in beforeListeners) {
                listener(e);
            }
            return e.Cancel;
        }

        public void After(T e) {
            foreach (var listener in afterListeners) {
                listener(e);
            }
        }
    }
    
}