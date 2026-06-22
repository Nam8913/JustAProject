using System.Collections.Generic;
using UnityEngine;

public static class EventQueue
{
    public static void Enqueue(string name, System.Action action)
    {
        queue.Enqueue(new EventQueueItem(name, action));
    }

    public static void ProcessNextEvent()
    {
        if(queue.Count > 0)
        {
            EventQueueItem item = queue.Dequeue();
            Debug.Log($"Processing event: {item.name}");
            item.action?.Invoke();
        }
    }

    public static Queue<EventQueueItem> queue = new Queue<EventQueueItem>();

    public class EventQueueItem
    {
        public string name;
        public System.Action action;
        public EventQueueItem(string name, System.Action action)
        {
            this.name = name;
            this.action = action;
        }
    }
}
