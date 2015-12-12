using System.Collections.Generic;

namespace AlphaMinerTest1
{
    public class EventLog
    {
        public IReadOnlyList<Trace> Traces { get; }

        public EventLog(IReadOnlyList<Trace> traces)
        {
            Traces = traces;
        }
    }
}