using System.Collections.Generic;
using System.Linq;

namespace ConferenceTimeTable
{
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
}