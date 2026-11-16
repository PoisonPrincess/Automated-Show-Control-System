using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace ShowAutomation.Domain.Models.Events
{
    [Serializable()]
    public class EventGroup : ISerializable
    {
        private List<Event> group = new List<Event>();
        private bool locked = false;

        public EventGroup() {
        }

        public EventGroup(SerializationInfo info, StreamingContext context) {
            locked = (bool)info.GetValue("isLocked", typeof(bool));
            group = (List<Event>)info.GetValue("group", typeof(List<Event>));
        }

        public Boolean Contains(Event ev) {
            return group.Contains(ev);
        }

        public Boolean Matches(List<Event> events) {
            if (events == null) {
                return false;
            }
            if (events.Count != group.Count) {
                return false;
            }
            for (int i = 0; i < events.Count; i++) {
                if (!group.Contains(events[i])) {
                    return false;
                }
            }
            return true;
        }

        public List<Event> GetElements() {
            return group;
        }

        public void Add(Event ev) {
            group.Add(ev);
        }

        public void AddAll(List<Event> events) {
            for (int i = 0; i < events.Count; i++) {
                group.Add(events[i]);
            }
        }

        public void Remove(Event ev) {
            group.Remove(ev);
        }

        public bool IsLocked()
        {
            return locked;
        }

        internal void updateTimesBy(long timeDiff)
        {
            long maxNegative = 0;
            long diffVal = 0;
            for (int x = 0; x < group.Count; x++)
            {
                diffVal = ((long)group[x].startTime + timeDiff);
                if ((diffVal < 0) && (diffVal < maxNegative))
                {
                    maxNegative = diffVal;
                }
            }
            for (int i = 0; i < group.Count; i++)
            {
                group[i].setStartTime((ulong)((long)group[i].startTime + (timeDiff - maxNegative)));
            }
        }

        internal void DragOffset(double movedAmount)
        {
            for (int i = 0; i < group.Count; i++)
            {
                double newLeft = Canvas.GetLeft(group[i].GetCanvas()) + movedAmount;
                if (newLeft < 0) {
                    newLeft = 0;
                }
                Canvas.SetLeft(group[i].GetCanvas(), newLeft);

            }
        }

        internal ulong GetMinStartTime()
        {
            ulong minTime = ulong.MaxValue;
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].startTime < minTime) {
                    minTime = group[i].startTime;
                }
            }
            return minTime;
        }

        internal ulong GetMaxEndTime()
        {
            ulong maxTime = 0;
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].getEndTime() > maxTime)
                {
                    maxTime = group[i].getEndTime();
                }
            }
            return maxTime;
        }

        internal void Lock(bool v)
        {
            locked = v;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("isLocked", locked);
            info.AddValue("group", group);
        }
    }
}
