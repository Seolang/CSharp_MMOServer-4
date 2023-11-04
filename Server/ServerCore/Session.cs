
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // sendArg 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnect(EndPoint endPoint);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnRecv(ArraySegment<byte> buffer);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv();

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        }


        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) 
                return;

            OnDisconnect(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            
        }
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        #region 네트워크 통신

        #region Send
        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                //_sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length)); // BufferList에 하나씩 Add 하면 안되고 
                _pendingList.Add(buff);
            }

            _sendArgs.BufferList = _pendingList; // List를 만든다음에 한번에 넣어줘야함

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // OnSendCompleted는 상위 RegisterSend가 lock 환경에서 명시적으로 실행되기 때문에 다시 lock을 걸 필요가 없음
            // 하지만 OnSendCompleted는 콜백함수로 실행되어 lock 환경에 존재하지 않으며,
            // 내부적으로 접근하는 데이터가 다른 곳에서도 접근 가능하다면 lock을 걸어 접근 불가능하게 만들어야 한다

            // (추가) 이미 상위 메소드에서 _lock에 대한 권한을 획득한 쓰레드는 이어서 해당 _lock에 접근할 수 있음
            lock (_lock) 
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null; // 꼭 초기화 할 필요는 없다
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend(); // 연속적으로 Queue를 비우기 위해 재귀
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleteFailed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        #endregion

        #region Receive
        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    RegisterRecv();
                }
                catch (Exception e) 
                {
                    Console.WriteLine($"OnRecvCompleteFailed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion

        #endregion
    }
}
