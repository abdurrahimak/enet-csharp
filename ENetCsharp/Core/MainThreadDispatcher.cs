using System;
using System.Collections.Generic;

namespace ENetCsharp
{
    internal static class MainThreadDispatcher
    {
        private static Dictionary<Guid, Queue<Action>> _actionQueueByGuid = new Dictionary<Guid, Queue<Action>>();

        public static void EnqueueAction(Guid guid, Action action)
        {
            if (_actionQueueByGuid.ContainsKey(guid))
            {
                _actionQueueByGuid[guid].Enqueue(action);
            }
            else
            {
                var q = new Queue<Action>();
                q.Enqueue(action);
                _actionQueueByGuid.Add(guid, q);
            }
        }

        public static Action DequeueAction(Guid guid)
        {
            if (_actionQueueByGuid.ContainsKey(guid))
            {
                if(_actionQueueByGuid[guid].Count > 0)
                {
                    return _actionQueueByGuid[guid].Dequeue();
                }
            }
            return null;
        }
    }

}
