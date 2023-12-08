using ServerCore;
using System.Net;
using System.Text;

namespace Server.Session
{
    /*
     * 서버측에서 클라이언트의 연결을 유지하는 클래스
     */
    class ClientSession : PacketSession // Session Inteface를 통해 다양한 세션 타입 정의 가능
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            // 게임 룸 접속을 JobQueue로 처리
            Program.Room.Push(() => Program.Room.Enter(this));
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnect(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this); // 세션 목록에서 삭제
            if (Room != null)
            {
                // 게임 룸 나가기 JobQueue 처리
                GameRoom room = Room; // 작업 예약 후 Room을 null로 밀어버려서 찾지를 못하게 됨, 따라서 객체 주소를 복사해놓는다
                room.Push(() => room.Leave(this)); 
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
