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

            PrintResult(sessions);

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
}
