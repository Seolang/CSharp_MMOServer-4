using System.Net;
using System.Text;
using Server.Session;
using ServerCore;

namespace Server
{
    /*
     * 메인 서버 프로그램
     */
    class Program
    {
        static Listener _listner = new Listener();
        public static GameRoom Room = new GameRoom();

        // 방의 패킷을 플러시 하는 메소드
        static void FlushRoom()
        {
            Room.Push(() => Room.Flush()); // 지금 당장 실행할 작업 JobQueue에 입력
            JobTimer.Instance.Push(FlushRoom, 250); // 0.25초 뒤에 재귀를 수행하라고 JobTimer에 예약
        }

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // ex) www.naver.com -> 123.456.789.012
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listner.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            //FlushRoom();
            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush(); // 메인 프로그램에서 JobTimer를 계속 실행해서 실행할 작업을 확인
            }
        }
    }
}