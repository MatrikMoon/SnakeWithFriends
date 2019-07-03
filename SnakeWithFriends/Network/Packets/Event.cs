using System;

namespace SnakeWithFriends.Network.Packets
{
    [Serializable]
    class Event
    {
        public enum EventType
        {
            Request,
            Resopnse,
            Event
        }

        public enum Events
        {
            Death
        }

        public enum Request
        {
            Spawn
        }

        public enum Response
        {
            SpawnCompleted
        }

        public int eventType;
        public int specificEvent;
    }
}
