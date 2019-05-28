using System;
using System.Collections.Generic;
using System.Linq;

namespace ConferenceTimeTable
{
    public class Session
    {
        public Session()
        {
            Talks = new List<Talk>();
        }

        public List<Talk> Talks { get; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public SessionType SessionType { get; set; }

        public int TotalAllocatedTalkDuration
        {
            get { return this.Talks.Sum(x => x.Duration); }
        }

        public int MaximumTotalSessionLength
        {
            get
            {
                TimeSpan interval = EndTime - StartTime;
                return (int)interval.TotalMinutes;
            }
        }

        public bool AddTalk(Talk talk)
        {
            if (TotalAllocatedTalkDuration <= MaximumTotalSessionLength)
            {
                this.Talks.Add(talk);
                return true;
            }

            return false;
        }
    }
}