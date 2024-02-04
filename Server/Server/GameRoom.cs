using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using ServerCore;

namespace Server
{
    /*
     * 여러 세션이 동시에 접속해 있는 단위
     */
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new();
        //object _lock = new object(); JobQueue에서 단일 쓰레드로 작업을 실행함이 보장되므로, 더이상 lock을 걸 필요가 없음
        JobQueue _jobQueue = new();
        List<ArraySegment<byte>> _pendingList = new();



        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        // 모았던 패킷을 한번에 처리하는 메소드
        public void Flush()
        {
            // 게임 룸에 참여한 모든 세션에게 pendingList에 들어있는 패킷 전송
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            // Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void BroadCast(ArraySegment<byte> segment)
        {
            // 패킷 모아 보내기
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가하고
            _sessions.Add(session);
            session.Room = this;

            // 신입생한테 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach(ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(players.Write()); // 현재 접속한 세션에게만 플레이어 리스트 전송

            // 신입생 입장을 모두에게 알린다
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame()
            {
                playerId = session.SessionId,
                posX = 0,
                posY = 0,
                posZ = 0,
            };
            BroadCast(enter.Write());
        }

        public void Leave(ClientSession session) 
        { 
            // 플레이어 제거
            _sessions.Remove(session);

            // 모두에게 알린다
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame()
            {
                playerId = session.SessionId,
            };
            BroadCast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 바꿔주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알린다
            S_BroadcastMove move = new S_BroadcastMove()
            {
                playerId = session.SessionId,
                posX = session.PosX,
                posY = session.PosY,
                posZ = session.PosZ,
            };
            BroadCast(move.Write());
        }
    }
}
