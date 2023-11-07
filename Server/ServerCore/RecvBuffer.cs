
namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos; // 여기부터 읽어야 하는 인덱스
        int _writePos; // 여기부터 써야 하는 인덱스

        // [r][][w][][][][][] buffer
        public RecvBuffer(int bufferSize)
        {
            // ArraySegment는 어떤 배열의 일부분을 참조해 가져오는 것
            // struct이므로 heap에 새로 할당하는 것이 아닌, stack에 저장한다 (메모리 가비지 부담이 적다)
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        // 아직 처리하지 않은 데이터 바이트 배열의 크기
        public int DataSize { get { return _writePos - _readPos; } }

        // 버퍼의 가용 사이즈 크기
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 아직 처리하지 않은 데이터 바이트 배열
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        // 수신 가능한 바이트 배열
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 데이터가 있으면 시작 위치로 복사해 당겨옴
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize); // length : 복사할 길이
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize) // 버퍼에 쓰여진 데이터보다 많이 읽은 잘못된 상황
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
