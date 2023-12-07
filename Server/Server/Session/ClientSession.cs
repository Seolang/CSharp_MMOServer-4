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

            // TODO
            Program.Room.Enter(this); // GameRoom 접속
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
                Room.Leave(this); // GameRoom 접속 해제
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");

        }
    }
}
