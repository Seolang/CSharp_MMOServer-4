using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    /*
     * 패킷 생성기
     */
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"
using ServerCore;
using System;
using System.Collections.Generic;

/*
* 패킷을 받았을 때 해당 패킷을 알맞게 조립하는 클래스
*/
class PacketManager
{{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return _instance; }} }} 
    #endregion

    PacketManager()
    {{
        Register();
    }}

    // 패킷 타입 별 생성 및 핸들링 메소드를 Dictionary로 미리 지정
    // Action : void delegate, Func : return값이 있는 delegate
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    // 패킷 종류 등록
    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {{
        // 패킷 사이즈와 ID를 가져옴
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // ID에 따라 해당 패킷의 종류에 알맞게 조립 후 해당 패킷 종류의 핸들러를 호출 
        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if(_makeFunc.TryGetValue(id, out func))
        {{
            IPacket packet = func.Invoke(session, buffer);

            // 기존에는 패킷 생성 후 바로 패킷 핸들러를 호출했지만, 이제는 커스텀 콜백함수를 지정할 수 있도록 변경
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }}
    }}

    // 패킷을 만드는 메소드, where로 T는 IPacket을 구현하고, new가 가능해야한다는 조건 지정
    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() 
    {{
        T packet = new T();
        packet.Read(buffer);

        return packet;
    }}

    public void HandlePacket(PacketSession session, IPacket packet)
    {{
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }}
}}
";

        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"
        _makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";

        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Net;
using System.Text;

public enum PacketID
{{
    {0}
}}

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";


        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";


        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort); // packet size offset
        count += sizeof(ushort); // packetId offset

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort); // packet size offset
        // insert packetId
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}

        // 패킷의 전체 바이트 길이 입력
        success &= BitConverter.TryWriteBytes(s, count);


        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}
";


        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat = 
@"public {0} {1};";


        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> s, ref ushort count)
    {{
        bool success = true;

        {4}

        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();
";


        // {0} 변수 이름
        // {1} To~변수 형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";


        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";


        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";


        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"
this.{1}s.Clear(); // 혹시나 skills에 더미 값이 있을 때를 위한 초기화
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for(int i = 0; i < {1}Len; i++) 
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    this.{1}s.Add({1});
}}
";


        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";


        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";


        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";


        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"
// 리스트 갯수 입력
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);

// 리스트 내용물 입력
foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Write(s, ref count);
}}
";

    }
}
