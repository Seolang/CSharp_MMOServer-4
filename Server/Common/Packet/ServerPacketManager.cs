
using ServerCore;

/*
* 패킷을 받았을 때 해당 패킷을 알맞게 조립하는 클래스
*/
class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // 패킷 타입 별 생성 및 핸들링 메소드를 Dictionary로 미리 지정
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    // 패킷 종류 등록
    public void Register()
    {
       _onRecv.Add((ushort)PacketID.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>);
        _handler.Add((ushort)PacketID.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler);


    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        // 패킷 사이즈와 ID를 가져옴
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // ID에 따라 해당 패킷의 종류에 알맞게 조립 후 해당 패킷 종류의 핸들러를 호출 
        Action<PacketSession, ArraySegment<byte>> action = null;
        if(_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    // 패킷을 만드는 메소드, where로 T는 IPacket을 구현하고, new가 가능해야한다는 조건 지정
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() 
    {
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
