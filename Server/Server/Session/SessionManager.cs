using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Session
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager(); // Singleton
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new();
        object _lock = new object();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                int sessionid = ++_sessionId;

                ClientSession session = new ClientSession();
                session.SessionId = sessionid;
                _sessions.Add(sessionid, session);

                Console.WriteLine($"Connected : {sessionid}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
