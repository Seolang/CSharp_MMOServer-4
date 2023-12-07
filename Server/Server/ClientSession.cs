using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
    /*
     * 서버측에서 클라이언트의 연결을 유지하는 클래스
     */
    class ClientSession : PacketSession // Session Inteface를 통해 다양한 세션 타입 정의 가능
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnect(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");

        }
    }
}
