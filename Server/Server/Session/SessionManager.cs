using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Session
{
    /*
     * 접속한 세션들을 관리하는 매니저
     */
    class SessionManager
    {
        static SessionManager _session = new SessionManager(); // Singleton
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new(); // 모든 세션 정보
        object _lock = new object();

        // 새로 접속한 세션 생성
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

        // 세션 검색
        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        // 세션 삭제
        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
