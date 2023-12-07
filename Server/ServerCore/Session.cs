
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // sealed를 붙이면 이 클래스를 상속받는 클래스는 해당 메소드를 override를 할 수 없다
        // [size(2)][packetId(2)][...][size(2)][packetId(2)][...]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            // 조립 가능한 모든 패킷을 처리
            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인 (Packet의 size 부분을 가져와서 확인)
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;

                // 다음 패킷 시작 부분을 시작점으로 하는 세그먼트를 buffer에 대체
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // sendArg 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnect(EndPoint endPoint);
        public abstract void OnSend(int numOfBytes);
        public abstract int OnRecv(ArraySegment<byte> buffer);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) 
                return;

            OnDisconnect(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        public void Send(ArraySegment<byte> sendBuff)
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
            if (_disconnected == 1)
                return;

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                //_sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length)); // BufferList에 하나씩 Add 하면 안되고 
                _pendingList.Add(buff);
            }

            _sendArgs.BufferList = _pendingList; // List를 만든다음에 한번에 넣어줘야함

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // pendingh이 false일때 OnSendCompleted는 상위 RegisterSend가 lock 환경에서 명시적으로 실행되기 때문에 다시 lock을 걸 필요가 없음
            // 하지만 pending이 true일 때 OnSendCompleted는 콜백함수로 실행되어 lock 환경에 존재하지 않으며,
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
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            // 사용 가능한 버퍼 공간을 할당 
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterReceive Failed {e}");
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동 (BytesTransffered : 수신된 바이트 크기)
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if ( processLen < 0 || _recvBuffer.DataSize < processLen) {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

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
