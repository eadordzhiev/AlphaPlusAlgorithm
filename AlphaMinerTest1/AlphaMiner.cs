using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlphaMinerTest1
{
    public static class AlphaMiner
    {
        public static PetriNet MinePetriNet(EventLog log)
        {
            // Pre-processing (Alpha+)
            var lengthOneLoops = FindLoopsOfLengthOne(log);
            var transitions = GetActivitiesFromEvents(log.Traces.SelectMany(trace => trace.Events))
                .Except(lengthOneLoops.Select(loop => loop.Activity))
                .ToList();
            
            // Alpha algorithm
            var maximalSet = FindMaximalSet(log, transitions).ToArray();

            var arcs = new List<Arc>();
            var places = new List<string>();

            for (int i = 0; i < maximalSet.Length; i++)
            {
                var pair = maximalSet[i];
                var place = i.ToString();
                places.Add(place);

                arcs.AddRange(pair.Item1.Select(transition => new Arc(transition, place)));
                arcs.AddRange(pair.Item2.Select(transition => new Arc(place, transition)));
            }

            const string inPlace = "start";
            places.Add(inPlace);
            var initialTransitions = GetActivitiesFromEvents(log.Traces.Select(trace => trace.Events.First()));
            arcs.AddRange(initialTransitions.Select(transition => new Arc(inPlace, transition)));

            const string outPlace = "end";
            places.Add(outPlace);
            var endTransitions = GetActivitiesFromEvents(log.Traces.Select(trace => trace.Events.Last()));
            arcs.AddRange(endTransitions.Select(transition => new Arc(transition, outPlace)));
            
            // Post-processing (Alpha+)
            foreach (var loop in lengthOneLoops)
            {
                var pairIndex = Array.FindIndex(maximalSet, pair =>
                    pair.Item1.Contains(loop.PrecedingActivity) &&
                    pair.Item2.Contains(loop.FollowingActivity));

                if (pairIndex == -1)
                {
                    continue;
                }

                var place = pairIndex.ToString();

                if (!transitions.Contains(loop.Activity))
                {
                    transitions.Add(loop.Activity);

                    arcs.Add(new Arc(place, loop.Activity));
                    arcs.Add(new Arc(loop.Activity, place));
                }
            }

            return new PetriNet(places, transitions, arcs);
        }

        private static LoopOfLengthOne[] FindLoopsOfLengthOne(EventLog log)
        {
            var result = new List<LoopOfLengthOne>();

            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 1; i++)
                {
                    if (events[i].Activity == events[i + 1].Activity)
                    {
                        int loopStart = i;
                        int loopEnd = events.Count - 1;
                        for (int j = i; j < events.Count - 1; j++)
                        {
                            if (events[j].Activity != events[j + 1].Activity)
                            {
                                loopEnd = j;
                                break;
                            }
                        }
                        
                        result.Add(new LoopOfLengthOne(
                            activity: events[i].Activity,
                            precedingActivity: events[loopStart - 1].Activity,
                            followingActivity: events[loopEnd + 1].Activity));

                        i = loopEnd;
                    }
                }
            }

            return result.Distinct().ToArray();
        }

        private static IEnumerable<string> GetActivitiesFromEvents(IEnumerable<Event> events)
        {
            return events.Select(@event => @event.Activity).Distinct();
        }
        
        private static IEnumerable<Tuple<string[], string[]>> FindMaximalSet(EventLog log, IEnumerable<string> transitions)
        {
            var footprintTable = new FootprintTable(log);
            var powerSet = transitions.PowerSet();

            var pairsSequence =
                from setA in powerSet
                from setB in powerSet
                where setA.Any() && setB.Any()
                where footprintTable.AreActivitiesConnected(setA, setB)
                select new Tuple<string[], string[]>(setA, setB);

            // To prevent multiple enumeration
            var pairs = pairsSequence.ToArray();

            var nonMaximalPlaces =
                from place1 in pairs
                from place2 in pairs
                where !Equals(place1, place2)
                where place1.Item1.ContainsAll(place2.Item1)
                && place1.Item2.ContainsAll(place2.Item2)
                select place2;

            return pairs.Except(nonMaximalPlaces).ToArray();
        }
    }
}
