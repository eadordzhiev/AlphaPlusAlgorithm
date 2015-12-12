using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaMinerTest1
{
    public enum RelationType
    {
        NotConnected = 0,
        Precedes,
        Follows,
        Parallel
    }

    public class FootprintTable
    {
        private readonly RelationType[,] _table;
        private readonly string[] _activities;

        public FootprintTable(EventLog log)
        {
            _activities = log.Traces.SelectMany(trace => trace.Events.Select(@event => @event.Activity)).Distinct().ToArray();
            _table = new RelationType[_activities.Length, _activities.Length];

            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 1; i++)
                {
                    var currentActivity = events[i].Activity;
                    var nextActivity = events[i + 1].Activity;

                    if (this[currentActivity, nextActivity] == RelationType.NotConnected)
                    {
                        this[currentActivity, nextActivity] = RelationType.Precedes;
                        this[nextActivity, currentActivity] = RelationType.Follows;
                    }
                    else if (this[currentActivity, nextActivity] == RelationType.Follows)
                    {
                        this[currentActivity, nextActivity] = this[nextActivity, currentActivity] = RelationType.Parallel;
                    }
                }
            }

            // Fix ordering relations for loops of length two
            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 2; i++)
                {
                    var currentActivity = events[i].Activity;
                    var nextActivity = events[i + 1].Activity;
                    var nextNextActivity = events[i + 2].Activity;

                    if (currentActivity == nextNextActivity)
                    {
                        this[currentActivity, nextActivity] = RelationType.Precedes;
                        this[nextActivity, currentActivity] = RelationType.Precedes;
                    }
                }
            }
        }

        public RelationType this[string activity1, string activity2]
        {
            get
            {
                var firstEventIndex = ActivityToMatrixIndex(activity1);
                var secondEventIndex = ActivityToMatrixIndex(activity2);

                return _table[firstEventIndex, secondEventIndex];
            }
            private set
            {
                var firstEventIndex = ActivityToMatrixIndex(activity1);
                var secondEventIndex = ActivityToMatrixIndex(activity2);

                _table[firstEventIndex, secondEventIndex] = value;
            }
        }
        
        public bool AreActivitiesConnected(IEnumerable<string> inputActivities, IEnumerable<string> outputActivities)
        {
            //(A,B), A = first, B = second
            var arrayFirst = inputActivities.ToArray();
            var arraySecond = outputActivities.ToArray();

            // For every a1,a2 in A => a1#a2
            for (int i = 0; i < arrayFirst.Length - 1; i++)
            {
                for (int j = i + 1; j < arrayFirst.Length; j++)
                {
                    if (this[arrayFirst[i], arrayFirst[j]] != RelationType.NotConnected)
                    {
                        return false;
                    }
                }
            }

            // For every b1, b2 in B => b1#b2
            for (int i = 0; i < arraySecond.Length - 1; i++)
            {
                for (int j = i + 1; j < arraySecond.Length; j++)
                {
                    if (this[arraySecond[i], arraySecond[j]] != RelationType.NotConnected)
                    {
                        return false;
                    }
                }
            }

            // For every a in A and b in B => a > b in f
            return arrayFirst.All(first => arraySecond.All(second => this[first, second] == RelationType.Precedes));
        }

        private int ActivityToMatrixIndex(string activity)
        {
            return Array.IndexOf(_activities, activity);
        }
    }
}