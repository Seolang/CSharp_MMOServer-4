using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        // 문지기
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog : 최대 대기수
            _listenSocket.Listen(10);

            //for(int i=0; i<10; i++) // 리스너를 여러개 두어 여러 연결을 동시에 받을 수도 있다
            //{
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // pending == true 일시 클라이언트의 접속 후 실행할 콜백 설정
                RegisterAccept(args);
            //}
        }

        void RegisterAccept(SocketAsyncEventArgs args)  // 클라이언트 접속을 연결하는 함수
        {
            args.AcceptSocket = null; // 이전 소켓 연결 정보를 지워줌

            bool pending = _listenSocket.AcceptAsync(args); // 비동기적으로 연결을 기다림

            if (pending == false) // 실행하자 마자 바로 클라이언트가 연결됨
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args) // 클라이언트 접속 후 콜백 함수
        {
            if (args.SocketError == SocketError.Success) // 에러 없이 잘 되었다
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args); // 다음 클라이언트 연결을 대기 시킴 (Register -> Complete -> Register 로 순환)
        }

        public Socket Accept() // 블로킹 방식의 Accept 함수 (사용하지 않음)
        {
            return _listenSocket.Accept(); // 블로킹 함수 (Accept 될 때 까지 프로그램이 중단됨)
        }
    }
}
