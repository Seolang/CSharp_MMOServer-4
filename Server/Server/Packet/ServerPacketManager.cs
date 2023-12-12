
using ServerCore;
using System;
using System.Collections.Generic;

/*
* 패킷을 받았을 때 해당 패킷을 알맞게 조립하는 클래스
*/
public class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } } 
    #endregion

    PacketManager()
    {
        Register();
    }

    // 패킷 타입 별 생성 및 핸들링 메소드를 Dictionary로 미리 지정
    // Action : void delegate, Func : return값이 있는 delegate
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    // 패킷 종류 등록
    public void Register()
    {

        _makeFunc.Add((ushort)PacketID.C_LeaveGame, MakePacket<C_LeaveGame>);
        _handler.Add((ushort)PacketID.C_LeaveGame, PacketHandler.C_LeaveGameHandler);


        _makeFunc.Add((ushort)PacketID.C_Move, MakePacket<C_Move>);
        _handler.Add((ushort)PacketID.C_Move, PacketHandler.C_MoveHandler);


    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        // 패킷 사이즈와 ID를 가져옴
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // ID에 따라 해당 패킷의 종류에 알맞게 조립 후 해당 패킷 종류의 핸들러를 호출 
        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if(_makeFunc.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer);

            // 기존에는 패킷 생성 후 바로 패킷 핸들러를 호출했지만, 이제는 커스텀 콜백함수를 지정할 수 있도록 변경
            if (onRecvCallback != null)
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    // 패킷을 만드는 메소드, where로 T는 IPacket을 구현하고, new가 가능해야한다는 조건 지정
    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() 
    {
        T packet = new T();
        packet.Read(buffer);

        return packet;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
