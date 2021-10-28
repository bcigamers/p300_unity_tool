using System;
using System.Collections.Generic;


public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, Action<Dictionary<string, object>>> eventDictionary;

    protected void Awake()
    {
        if (eventDictionary == null)
            eventDictionary = new Dictionary<string, Action<Dictionary<string, object>>>();
    }

    public void StartListening(string eventName, Action<Dictionary<string, object>> listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Action<Dictionary<string, object>> thisEvent))
        {
            thisEvent += listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StopListening(string eventName, Action<Dictionary<string, object>> listener)
    {
        Action<Dictionary<string, object>> thisEvent;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public void TriggerEvent(string eventName, Dictionary<string, object> message)
    {
        Action<Dictionary<string, object>> thisEvent;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent?.Invoke(message);
        }
    }
}