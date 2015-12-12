using System;

namespace AlphaMinerTest1
{
    public class Event
    {
        public string Activity { get; }

        public string Name { get; }

        public string Transition { get; }

        public DateTime Timestamp { get; }

        public string Resource { get; }
        
        public Event(string name)
        {
            Name = name;
        }

        public Event(string name, string transition, DateTime timestamp, string resource)
        {
            Name = name;
            Transition = transition;
            Timestamp = timestamp;
            Resource = resource;

            Activity = Name + '+' + Transition;
        }
    }
}