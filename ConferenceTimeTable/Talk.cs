using System;

namespace ConferenceTimeTable
{
    public class Talk
    {
        public string Title { get; set; }

        public int Duration { get; set; }

        public DateTime StarTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool isAllocated { get; set; }

        public string FormattedStartTime => this.StarTime.ToString("HH:mm");

        public string FormattedEndTime => this.EndTime.ToString("HH:mm");
    }
}