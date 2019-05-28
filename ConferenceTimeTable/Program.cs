using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConferenceTimeTable
{
    public class Program
    {
        public const string StartTimeMorning = "09:00";
        public const string EndTimeTimeMorning = "12:00";
        public const string StartTimeAfternoon = "13:00";
        public const string EndTimeTimeAfternoon = "17:00";

        static void Main(string[] args)
        {
            List<Talk> talkList;

            string input;
            Console.WriteLine("Enter file path for input or default file in path 'C:\\Temp\\input.txt' will be used: ");
            var inputPath = Console.ReadLine();

            if (inputPath.Length > 0)
            {
                talkList = ParseFile(inputPath);
            }
            else
            {
                talkList = ParseFile();
            }

            var tracks = GenerateTimeTableList(talkList.ToArray());
            PrintResult(tracks);
        }

        private static void PrintResult(Session[] sessions)
        {
            for (int t = 0; t <= sessions.Length - 1; t++)
            {
                var currentSession = sessions[t];

                Console.WriteLine($"Session {t + 1}");
                for (int j = 0; j <= sessions[t].Talks.Count - 1; j++)
                {
                    var currentTalk = currentSession.Talks[j];
                    if (j == 0)
                    {

                        currentTalk.StarTime = currentSession.StartTime;
                        currentTalk.EndTime = currentTalk.StarTime.AddMinutes(currentTalk.Duration);
                    }
                    else
                    {
                        currentTalk.StarTime = currentSession.Talks[j - 1].EndTime;
                        currentTalk.EndTime = currentTalk.StarTime.AddMinutes(currentTalk.Duration);
                    }

                    Console.WriteLine($"{currentTalk.FormattedStartTime} - {currentTalk.FormattedEndTime}  {currentTalk.Title}");

                    if (j == sessions[t].Talks.Count - 1)
                    {
                        if (currentSession.SessionType == SessionType.Morning)
                        {
                            Console.WriteLine($"{currentSession.EndTime:HH:mm} Lunch");
                        }
                        else
                        {
                            Console.WriteLine($"{currentSession.EndTime:HH:mm} Networking Event");
                        }
                    }

                }

            }

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }

        public static List<Talk> ParseFile(string filepath = "")
        {
            if (filepath == string.Empty)
            {
                filepath = "C:\\Temp\\input.txt";
            }

            List<Talk> talkList = new List<Talk>();
            using (TextReader rdr = File.OpenText(filepath))
            {
                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    if (line.Contains("lightning"))
                    {
                        Talk talk = new Talk();
                        talk.Duration = 5;
                        talk.Title = line.Remove(line.IndexOf("lightning"), "lightning".Length).Trim();
                        talkList.Add(talk);
                    }
                    else
                    {
                        var matches = Regex.Matches(line, @"(\d+)");

                        if (matches.Count > 0)
                        {
                            var number = matches[0].Value;
                            var description = line.Remove(line.IndexOf(number), number.Length + 3).Trim();

                            Talk talk = new Talk();
                            talk.Duration = Convert.ToInt32(number);
                            talk.Title = description;
                            talkList.Add(talk);
                        }

                    }
                }
            }

            return talkList;
        }

        public static Session[] GenerateTimeTableList(Talk[] talkArray)
        {
            Session[] sessions = InitializeSessions(4);
            int sessionNumber = 0;
            int totalTalkLength = 0;

            for (int i = 0; i <= talkArray.Length - 1; i++)
            {
                int maximumTotalSessionLength = sessions[sessionNumber].MaximumTotalSessionLength;

                if (i == 0)
                {
                    if (talkArray[i].Duration <= maximumTotalSessionLength)
                    {
                        totalTalkLength = talkArray[i].Duration;
                        if (talkArray[i].isAllocated == false)
                        {
                            sessions[sessionNumber].Talks.Add(talkArray[i]);
                            talkArray[i].isAllocated = true;
                        }

                    }

                }
                else
                {
                    if (totalTalkLength + talkArray[i].Duration <= maximumTotalSessionLength)
                    {
                        totalTalkLength = totalTalkLength + talkArray[i].Duration;
                        if (talkArray[i].isAllocated == false)
                        {
                            sessions[sessionNumber].Talks.Add(talkArray[i]);
                            talkArray[i].isAllocated = true;
                        }

                    }
                    else
                    {
                        bool isSlotfound = false;
                        //iterate over remaining sessions to see if there is a slot available
                        //for (int j = i; j < talkArray.Length - 1; j++)
                        //{
                        //    if (totalTalkLength + talkArray[j].Duration <= maximumTotalSessionLength)
                        //    {
                        //        totalTalkLength = totalTalkLength + talkArray[j].Duration;
                        //        if (talkArray[j].isAllocated == false)
                        //        {
                        //            sessions[sessionNumber].Talks.Add(talkArray[j]);
                        //            talkArray[j].isAllocated = true;
                        //        }

                        //        isSlotfound = true;
                        //    }
                        //}

                        var unallocatedtTalks = talkArray.Where(x => x.isAllocated == false).ToArray();

                        for (int j = 0; j < unallocatedtTalks.Length - 1; j++)
                        {
                            if (totalTalkLength + talkArray[j].Duration <= maximumTotalSessionLength)
                            {
                                totalTalkLength = totalTalkLength + talkArray[j].Duration;
                                if (talkArray[j].isAllocated == false)
                                {
                                    sessions[sessionNumber].Talks.Add(talkArray[j]);
                                    talkArray[j].isAllocated = true;
                                }

                                isSlotfound = true;
                            }

                            if (isSlotfound == false)
                            {
                                // iterate existing sessions to find a matching slot
                                bool slotFoundInOtherTracks = false;
                                for (int t = 0; t <= sessionNumber - 1; t++)
                                {
                                    if (sessions[t].TotalTalkDuration + unallocatedtTalks[j].Duration <= sessions[t].MaximumTotalSessionLength)
                                    {
                                        if (unallocatedtTalks[j].isAllocated == false)
                                        {
                                            sessions[t].Talks.Add(unallocatedtTalks[j]);
                                            unallocatedtTalks[j].isAllocated = true;
                                        }

                                        slotFoundInOtherTracks = true;
                                    }
                                }

                                // If no slots found in existing sessions,
                                // use next track
                                if (slotFoundInOtherTracks == false)
                                {
                                    if (sessionNumber < sessions.Length - 1)
                                    {
                                        sessionNumber++;
                                        totalTalkLength = unallocatedtTalks[j].Duration;
                                        if (unallocatedtTalks[j].isAllocated == false)
                                        {
                                            sessions[sessionNumber].Talks.Add(unallocatedtTalks[j]);
                                            unallocatedtTalks[j].isAllocated = true;
                                        }

                                    }

                                }

                            }
                        }


                        

                    }
                }

            }

            return sessions;
        }

        public static Session[] InitializeSessions(int numberOfSessions)
        {
            Session[] sessions = new Session[numberOfSessions];

            for (int i = 0; i < numberOfSessions; i++)
            {
                sessions[i] = new Session();

                if (i % 2 == 0)
                {
                    //morning session
                    sessions[i].SessionType = SessionType.Morning;
                    sessions[i].StartTime = DateTime.ParseExact(StartTimeMorning, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    sessions[i].EndTime = DateTime.ParseExact(EndTimeTimeMorning, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    sessions[i].SessionType = SessionType.Afternoon;
                    sessions[i].StartTime = DateTime.ParseExact(StartTimeAfternoon, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    sessions[i].EndTime = DateTime.ParseExact(EndTimeTimeAfternoon, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return sessions;
        }

    }

    public class Conference
    {
        public Conference()
        {
            Tracks = new List<Track>();
        }
        public List<Track> Tracks { get; }
    }

    public class Track
    {
        public List<Session> Sessions {get; set;}

        public Track()
        {
            Sessions = new List<Session>();
        }
    }

    public class Session
    {
        public Session()
        {
            Talks = new List<Talk>();
        }

        public List<Talk> Talks { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public SessionType SessionType { get; set; }

        public int TotalTalkDuration
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
    }

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

    public enum SessionType
    {
        Morning = 1,
        Afternoon
    }

    
}
