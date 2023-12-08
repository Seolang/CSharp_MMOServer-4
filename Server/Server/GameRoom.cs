using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new();
        //object _lock = new object(); JobQueue에서 단일 쓰레드로 작업을 실행함이 보장되므로, 더이상 lock을 걸 필요가 없음
        JobQueue _jobQueue = new();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void BroadCast(ClientSession session, string chat)
        {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            //lock (_lock)
            //{
                // N ^ 2 시간이 걸린다 => 브로드캐스팅과 같은 작업은 패킷 모아보내기가 필수
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            //}
        }

        public void Enter(ClientSession session)
        {
            //lock (_lock) // 멀티스레드 대비
            //{
                _sessions.Add(session);
                session.Room = this;
            //}
        }

        public void Leave(ClientSession session) 
        { 
            //lock (_lock)
            //{
                _sessions.Remove(session);
            //}
        }
    }
}
