using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    private static EventBus instance;
    public static EventBus Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EventBus");
                instance = go.AddComponent<EventBus>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<Type, List<Action<object>>> eventHandlers = new Dictionary<Type, List<Action<object>>>();

    public void Subscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);
        Action<object> wrapper = (obj) => handler((T)obj);

        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = new List<Action<object>>();
        }
        eventHandlers[eventType].Add(wrapper);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);
        if (eventHandlers.ContainsKey(eventType))
        {
            Action<object> wrapper = (obj) => handler((T)obj);
            eventHandlers[eventType].Remove(wrapper);
        }
    }

    public void Publish<T>(T eventData)
    {
        Type eventType = typeof(T);
        if (eventHandlers.ContainsKey(eventType))
        {
            foreach (var handler in eventHandlers[eventType])
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error handling event {eventType}: {e}");
                }
            }
        }
    }

    private void OnDestroy()
    {
        eventHandlers.Clear();
        instance = null;
    }
}
