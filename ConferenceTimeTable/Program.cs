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

            Console.WriteLine("Enter file path for input or default file in path 'C:\\Temp\\input.txt' will be used: ");

            var inputPath = Console.ReadLine();

            var talkList = string.IsNullOrEmpty(inputPath) ? ParseFile() : ParseFile(inputPath);

            var sessions = GenerateTimeTableList(talkList.ToArray());

            Conference conference = new Conference(sessions);

            //PrintResult(sessions);

            PrintResult(conference);

            //var talkList = ParseFile("C:\\Temp\\input.txt");

            //var tracks = GenerateTimeTableList(talkList.ToArray());

            //PrintResult(tracks);



        }

        private static void PrintResult(Conference conference)
        {
            int conferenceTrackNumber = 0;
            foreach (var conferenceTrack in conference.Tracks)
            {
                conferenceTrackNumber++;
                Console.WriteLine($"Track {conferenceTrackNumber}");
                foreach (var conferenceTrackSession in conferenceTrack.Sessions)
                {

                    for (int j = 0; j <= conferenceTrackSession.Talks.Count - 1; j++)
                    {
                        var currentTalk = conferenceTrackSession.Talks[j];

                        if (j == 0)
                        {
                            currentTalk.StarTime = conferenceTrackSession.StartTime;
                            currentTalk.EndTime = currentTalk.StarTime.AddMinutes(currentTalk.Duration);
                        }
                        else
                        {
                            currentTalk.StarTime = conferenceTrackSession.Talks[j - 1].EndTime;
                            currentTalk.EndTime = currentTalk.StarTime.AddMinutes(currentTalk.Duration);
                        }

                        Console.WriteLine($"{currentTalk.FormattedStartTime} - {currentTalk.FormattedEndTime}  {currentTalk.Title}");

                        if (j == conferenceTrackSession.Talks.Count - 1)
                        {
                            Console.WriteLine(conferenceTrackSession.SessionType == SessionType.Morning
                                ? $"{conferenceTrackSession.EndTime:HH:mm} Lunch"
                                : $"{conferenceTrackSession.EndTime:HH:mm} Networking Event");
                        }

                    }

                    /*int talkNumber = 0;
                    foreach (var talk in conferenceTrackSession.Talks)
                    {
                        if (talkNumber == 0)
                        {
                            talk.StarTime = conferenceTrackSession.StartTime;
                            talk.EndTime = talk.StarTime.AddMinutes(talk.Duration);
                        }
                        else
                        {
                            //talk.StarTime = currentSession.Talks[j - 1].EndTime;
                            //currentTalk.EndTime = currentTalk.StarTime.AddMinutes(currentTalk.Duration);
                        }

                        Console.WriteLine($"{talk.FormattedStartTime} - {talk.FormattedEndTime}  {talk.Title}");
                        talkNumber++;
                    }*/
                }
            }

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
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
                        Console.WriteLine(currentSession.SessionType == SessionType.Morning
                            ? $"{currentSession.EndTime:HH:mm} Lunch"
                            : $"{currentSession.EndTime:HH:mm} Networking Event");
                    }

                }

            }

            /*Console.WriteLine("Press enter to close...");
            Console.ReadLine();*/
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
                        Talk talk = new Talk
                        {
                            Duration = 5,
                            Title = line.Remove(line.IndexOf("lightning", StringComparison.Ordinal), "lightning".Length).Trim()
                        };
                        talkList.Add(talk);
                    }
                    else
                    {
                        var matches = Regex.Matches(line, @"(\d+)");

                        if (matches.Count > 0)
                        {
                            var number = matches[0].Value;
                            var description = line.Remove(line.IndexOf(number, StringComparison.Ordinal), number.Length + 3).Trim();

                            Talk talk = new Talk {Duration = Convert.ToInt32(number), Title = description};
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

            foreach (var talk in talkArray)
            {
                if (sessions[sessionNumber].AddTalk(talk))
                {
                    talk.isAllocated = true;

                }
                else
                {
                    //increment the session
                    if (sessionNumber <= sessions.Length - 1)
                    {
                        sessionNumber++;
                    }
                }
            }

            var unallocatedTalks = talkArray.Where(x => x.isAllocated == false).ToArray();

            foreach (var talk in unallocatedTalks)
            {
                foreach (var session in sessions)
                {
                    if (session.AddTalk(talk))
                    {
                        talk.isAllocated = true;
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

        public Conference(Session[] sessions)
        {
            Tracks = new List<Track>();

            foreach (var session in sessions)
            {
                if (!Tracks.Any())
                {
                    Track track = new Track();
                    track.AddSession(session);
                    Tracks.Add(track);
                }
                else
                {
                    var track = Tracks.FirstOrDefault(x => x.Sessions.Count<Session>() != 2);

                    if (track != null && track.AddSession(session))
                    {
                        Tracks.Add(track);
                    }
                }

            }
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

        public bool AddSession(Session session)
        {
            if (Sessions.Count(x => x.SessionType == session.SessionType) <= 0)
            {
                Sessions.Add(session);
                return true;
            }

            return false;
        }
    }

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
