using System.Net;
using System.Text;
using Server.Session;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listner = new Listener();
        public static GameRoom Room = new GameRoom();

        static void Main(string[] args)
        {
            PacketManager.Instance.Register(); // 패킷 매니저 초기설정

            // DNS (Domain Name System)
            // ex) www.naver.com -> 123.456.789.012
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listner.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            while (true)
            {
                ; // 프로그램이 종료되지 않게 유지
            }
        }
    }
}