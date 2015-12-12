using System.Collections.Generic;

namespace AlphaMinerTest1
{
    public class Trace
    {
        public IReadOnlyList<Event> Events { get; }

        public Trace(IReadOnlyList<Event> events)
        {
            Events = events;
        }
    }
}